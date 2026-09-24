using System;
using System.Collections.Concurrent;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Data.ConnectionStrings;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

[Dependency(AutoRegister = false)]
public class MongoDbContextProvider<TContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IUnitOfWorkManager unitOfWorkManager,
    IOptions<AxiomDbContextOptions<TContext>> options,
    IConnectionStringProvider connectionStringProvider,
    DbContextOptions<TContext> dbContextOptions)
    : IDbContextProvider<TContext>, IDisposable
    where TContext : DbContext
{
    protected IDbContextFactory<TContext> DbContextFactory { get; } = dbContextFactory;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected AxiomDbContextOptions<TContext> Options { get; } = options.Value;
    protected IConnectionStringProvider ConnectionStringProvider { get; } = connectionStringProvider;
    protected DbContextOptions<TContext> DbContextOptions { get; } = dbContextOptions;
    protected ConcurrentDictionary<string, DbContextOptions<TContext>> DbContextOptionsCache { get; } = [];
    protected ConcurrentQueue<IMongoClient> Clients { get; } = new();

    protected ObjectFactory<TContext> Factory { get; } =
        ActivatorUtilities.CreateFactory<TContext>([typeof(DbContextOptions<TContext>)]);

    public async ValueTask<TContext> GetAsync(CancellationToken cancellationToken = default)
    {
        var unitOfWork = UnitOfWorkManager.RequiredCurrent;
        cancellationToken = cancellationToken.FallbackTo(unitOfWork.CancellationToken);

        var connectionString = await ConnectionStringProvider.FindAsync(Options.ConnectionStringName);
        var key = $"{typeof(TContext).FullName!}_{connectionString}"; //TODO: We might optimize here
        if (unitOfWork.Databases.TryGetValue(key, out var dbHandle))
        {
            return dbHandle.GetDatabase<TContext>();
        }

        var dbContext = await CreateDbContextAsync(unitOfWork, connectionString, cancellationToken);
        await AddDatabaseHandleAsync(unitOfWork, key, dbContext, cancellationToken);

        return dbContext;
    }

    protected virtual async ValueTask<TContext> CreateDbContextAsync(
        IUnitOfWork unitOfWork,
        string? connectionString,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return await DbContextFactory.CreateDbContextAsync(cancellationToken);
        }

        var dbContextOptions = GetDbContextOptions(connectionString);
        var dbContext = Factory(unitOfWork.ServiceProvider, [dbContextOptions]);
        // MongoDB has no transaction-wide command timeout equivalent, so UnitOfWorkOptions.Timeout can't be applied.

        return dbContext;
    }

    protected virtual DbContextOptions<TContext> GetDbContextOptions(string connectionString)
    {
        return DbContextOptionsCache.GetOrAdd(connectionString, static (key, state) =>
        {
            var builder = new DbContextOptionsBuilder<TContext>(state.DbContextOptions);
            var url = new MongoUrl(key);
            var client = new MongoClient(url);

            state.Clients.Enqueue(client);
            builder.UseMongoDB(client, url.DatabaseName);

            return builder.Options;
        }, (DbContextOptions, Clients));
    }

    protected virtual async Task AddDatabaseHandleAsync(
        IUnitOfWork unitOfWork,
        string key,
        TContext dbContext,
        CancellationToken cancellationToken = default)
    {
        UnitOfWorkDatabaseHandle handle;

        if (unitOfWork.Options.IsolationLevel.HasValue)
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(
                MapToMongoTransactionOptions(unitOfWork.Options.IsolationLevel.Value),
                cancellationToken);
            handle = new UnitOfWorkDatabaseHandle(
                dbContext,
                transaction,
                UnitOfWorkDatabaseHandleExtensions.SaveChangesAsync,
                UnitOfWorkDatabaseHandleExtensions.CommitAsync,
                UnitOfWorkDatabaseHandleExtensions.RollbackAsync);
        }
        else if (unitOfWork.Options.TransactionBehavior == UnitOfWorkTransactionBehavior.Suppress)
        {
            handle = new UnitOfWorkDatabaseHandle(dbContext, UnitOfWorkDatabaseHandleExtensions.SaveChangesAsync);
        }
        else
        {
            handle = new UnitOfWorkDatabaseHandle(
                dbContext,
                UnitOfWorkDatabaseHandleExtensions.SaveChangesAsync,
                // When IsolationLevel exists it handled in first if condition
                UnitOfWorkDatabaseHandleExtensions.BeginTransactionAsync,
                UnitOfWorkDatabaseHandleExtensions.CommitAsync,
                UnitOfWorkDatabaseHandleExtensions.RollbackAsync);
        }

        unitOfWork.AddDatabase(key, handle);
    }

    protected virtual TransactionOptions MapToMongoTransactionOptions(IsolationLevel isolationLevel)
    {
        var readConcern = isolationLevel switch
        {
            IsolationLevel.ReadUncommitted => ReadConcern.Local,
            IsolationLevel.ReadCommitted => ReadConcern.Majority,
            IsolationLevel.RepeatableRead => ReadConcern.Snapshot,
            IsolationLevel.Snapshot => ReadConcern.Snapshot,
            IsolationLevel.Serializable => ReadConcern.Snapshot,
            _ => ReadConcern.Majority
        };

        return new TransactionOptions(
            readConcern: readConcern,
            writeConcern: WriteConcern.WMajority
        );
    }

    public virtual void Dispose()
    {
        while (Clients.TryDequeue(out var client))
        {
            client.Dispose();
        }
    }
}