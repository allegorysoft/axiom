using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Allegory.Axiom.EntityFrameworkCore;

public class DbContextProvider<TContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IUnitOfWorkManager unitOfWorkManager)
    : IDbContextProvider<TContext>, ISingletonService
    where TContext : DbContext
{
    protected IDbContextFactory<TContext> DbContextFactory { get; } = dbContextFactory;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    public virtual async ValueTask<TContext> GetAsync(CancellationToken cancellationToken = default)
    {
        // GetRequestedDbContext (for interface find underlying db context they might replace)
        // ResolveConnectionString
        // Check database exists in uow (context.Type_ConnectionStr) => hash 
        // CreateDbContext; SetConnection

        var unitOfWork = UnitOfWorkManager.RequiredCurrent;
        cancellationToken = cancellationToken.FallbackTo(unitOfWork.CancellationToken);
        var key = typeof(TContext).FullName!;

        if (unitOfWork.Databases.TryGetValue(key, out var dbHandle))
        {
            return dbHandle.GetDatabase<TContext>();
        }

        var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        if (unitOfWork.Options.IsolationLevel.HasValue)
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            dbHandle = new UnitOfWorkDatabaseHandle(
                dbContext, 
                transaction,
                SaveChangesAsync,
                CommitAsync,
                RollbackAsync);
        }
        else if (unitOfWork.Options.TransactionBehavior == UnitOfWorkTransactionBehavior.Suppress)
        {
            dbHandle = new UnitOfWorkDatabaseHandle(dbContext, SaveChangesAsync);
        }
        else
        {
            dbHandle = new UnitOfWorkDatabaseHandle(
                dbContext,
                SaveChangesAsync,
                BeginTransactionAsync,
                CommitAsync,
                RollbackAsync);
        }

        unitOfWork.AddDatabase(key, dbHandle);
        return dbContext;
    }

    protected static Task SaveChangesAsync(
        UnitOfWorkDatabaseHandle dbHandle,
        CancellationToken cancellationToken = default)
    {
        return dbHandle.GetDatabase<DbContext>().SaveChangesAsync(cancellationToken);
    }

    protected static async Task<object> BeginTransactionAsync(
        UnitOfWorkDatabaseHandle dbHandle,
        CancellationToken cancellationToken = default)
    {
        return await dbHandle.GetDatabase<DbContext>().Database.BeginTransactionAsync(cancellationToken);
    }

    protected static Task CommitAsync(
        UnitOfWorkDatabaseHandle dbHandle,
        CancellationToken cancellationToken = default)
    {
        return dbHandle.GetTransaction<IDbContextTransaction>().CommitAsync(cancellationToken);
    }

    protected static Task RollbackAsync(
        UnitOfWorkDatabaseHandle dbHandle,
        CancellationToken cancellationToken = default)
    {
        return dbHandle.GetTransaction<IDbContextTransaction>().RollbackAsync(cancellationToken);
    }
}