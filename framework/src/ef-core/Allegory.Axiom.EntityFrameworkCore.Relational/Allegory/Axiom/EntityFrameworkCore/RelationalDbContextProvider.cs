using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Data.ConnectionStrings;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EntityFrameworkCore;

public class RelationalDbContextProvider<TContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IUnitOfWorkManager unitOfWorkManager,
    IOptions<AxiomDbContextOptions<TContext>> options,
    IConnectionStringProvider connectionStringProvider)
    : IDbContextProvider<TContext>, ISingletonService
    where TContext : DbContext
{
    protected IDbContextFactory<TContext> DbContextFactory { get; } = dbContextFactory;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected AxiomDbContextOptions<TContext> Options { get; } = options.Value;
    protected IConnectionStringProvider ConnectionStringProvider { get; } = connectionStringProvider;

    public virtual async ValueTask<TContext> GetAsync(CancellationToken cancellationToken = default)
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
        var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            dbContext.Database.SetConnectionString(connectionString);
        }

        if (unitOfWork.Options.Timeout.HasValue)
        {
            dbContext.Database.SetCommandTimeout(unitOfWork.Options.Timeout.Value);
        }

        return dbContext;
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
                unitOfWork.Options.IsolationLevel.Value,
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
}