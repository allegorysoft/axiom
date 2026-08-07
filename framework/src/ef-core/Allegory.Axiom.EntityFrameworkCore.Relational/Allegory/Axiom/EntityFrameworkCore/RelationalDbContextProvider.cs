using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Data;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public class RelationalDbContextProvider<TContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IUnitOfWorkManager unitOfWorkManager,
    IConnectionStringProvider connectionStringProvider)
    : IDbContextProvider<TContext>, ISingletonService
    where TContext : DbContext
{
    public IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected IDbContextFactory<TContext> DbContextFactory { get; } = dbContextFactory;
    protected IConnectionStringProvider ConnectionStringProvider { get; } = connectionStringProvider;

    public async ValueTask<TContext> GetAsync(CancellationToken cancellationToken = default)
    {
        // GetRequestedDbContext (for interface find underlying db context they might replace)
        // ResolveConnectionString
        // Check database exists in uow (context.Type_ConnectionStr) => hash 
        // CreateDbContext; SetConnection
        
        //uow.Items.TryGetValue("db_{tenant.current.id??host}_{dbkey}")

        var unitOfWork = UnitOfWorkManager.RequiredCurrent;
        cancellationToken = cancellationToken.FallbackTo(unitOfWork.CancellationToken);
        var key = typeof(TContext).FullName!;

        if (unitOfWork.Databases.TryGetValue(key, out var dbHandle))
        {
            return dbHandle.GetDatabase<TContext>();
        }

        var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        var connectionString = ConnectionStringProvider.GetAsync();
        //dbContext.Database.SetConnectionString();

        if (unitOfWork.Options.IsolationLevel.HasValue)
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(
                unitOfWork.Options.IsolationLevel.Value,
                cancellationToken);
            dbHandle = new UnitOfWorkDatabaseHandle(
                dbContext,
                transaction,
                UnitOfWorkDatabaseHandleExtensions.SaveChangesAsync,
                UnitOfWorkDatabaseHandleExtensions.CommitAsync,
                UnitOfWorkDatabaseHandleExtensions.RollbackAsync);
        }
        else if (unitOfWork.Options.TransactionBehavior == UnitOfWorkTransactionBehavior.Suppress)
        {
            dbHandle = new UnitOfWorkDatabaseHandle(dbContext, UnitOfWorkDatabaseHandleExtensions.SaveChangesAsync);
        }
        else
        {
            dbHandle = new UnitOfWorkDatabaseHandle(
                dbContext,
                UnitOfWorkDatabaseHandleExtensions.SaveChangesAsync,
                // When IsolationLevel exists it handled in first if condition
                UnitOfWorkDatabaseHandleExtensions.BeginTransactionAsync,
                UnitOfWorkDatabaseHandleExtensions.CommitAsync,
                UnitOfWorkDatabaseHandleExtensions.RollbackAsync);
        }

        unitOfWork.AddDatabase(key, dbHandle);
        return dbContext;
    }
}