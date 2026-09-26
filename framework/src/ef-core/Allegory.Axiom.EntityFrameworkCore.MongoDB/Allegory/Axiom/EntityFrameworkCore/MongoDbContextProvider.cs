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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public class MongoDbContextProvider<TContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IUnitOfWorkManager unitOfWorkManager,
    IOptions<AxiomDbContextOptions<TContext>> options,
    IConnectionStringProvider connectionStringProvider,
    DbContextOptions<TContext> dbContextOptions) :
    DbContextProvider<TContext>(dbContextFactory, unitOfWorkManager, options, connectionStringProvider)
    where TContext : DbContext
{
    protected DbContextOptions<TContext> DbContextOptions { get; } = dbContextOptions;
    protected ConcurrentDictionary<string, DbContextOptions<TContext>> DbContextOptionsCache { get; } = [];
    protected ConcurrentQueue<IMongoClient> Clients { get; } = new();

    protected ObjectFactory<TContext> Factory { get; } =
        ActivatorUtilities.CreateFactory<TContext>([typeof(DbContextOptions<TContext>)]);

    protected override async ValueTask<TContext> CreateDbContextAsync(
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

    protected override async Task TryBeginTransactionAsync(
        IUnitOfWork unitOfWork,
        TContext dbContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Database.BeginTransactionAsync(
                MapToMongoTransactionOptions(unitOfWork.Options.IsolationLevel!.Value),
                cancellationToken);
        }
        catch (NotSupportedException e)
        {
            var logger = unitOfWork.ServiceProvider.GetRequiredService<ILogger<UnitOfWorkDbHandle>>();
            logger.LogWarning(e, "Transaction not supported for {DbContext}", typeof(TContext));
        }
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