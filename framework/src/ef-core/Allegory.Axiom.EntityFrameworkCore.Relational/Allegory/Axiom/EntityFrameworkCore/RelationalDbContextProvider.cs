using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public class RelationalDbContextProvider<TContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IUnitOfWorkManager unitOfWorkManager)
    : DbContextProvider<TContext>(dbContextFactory, unitOfWorkManager)
    where TContext : DbContext
{
    public override async ValueTask<TContext> GetAsync(CancellationToken cancellationToken = default)
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
        //dbContext.Database.SetConnectionString();

        if (unitOfWork.Options.IsolationLevel.HasValue)
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(
                unitOfWork.Options.IsolationLevel.Value,
                cancellationToken);
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
                BeginTransactionWithIsolationAsync,
                CommitAsync,
                RollbackAsync);
        }

        unitOfWork.AddDatabase(key, dbHandle);
        return dbContext;
    }

    protected static async Task<object> BeginTransactionWithIsolationAsync(
        UnitOfWorkDatabaseHandle dbHandle,
        CancellationToken cancellationToken = default)
    {
        if (dbHandle.UnitOfWork.Options.IsolationLevel.HasValue)
        {
            return await dbHandle.GetDatabase<DbContext>().Database.BeginTransactionAsync(
                dbHandle.UnitOfWork.Options.IsolationLevel!.Value,
                cancellationToken);
        }

        return await dbHandle.GetDatabase<DbContext>().Database.BeginTransactionAsync(cancellationToken);
    }
}