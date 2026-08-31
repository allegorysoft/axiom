using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Data;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EntityFrameworkCore;

public class RelationalDbContextProvider<TContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IUnitOfWorkManager unitOfWorkManager,
    ITenantContextAccessor tenantContextAccessor,
    IOptions<AxiomDbContextOptions<TContext>> options,
    IConnectionStringProvider connectionStringProvider)
    : IDbContextProvider<TContext>, ISingletonService
    where TContext : DbContext
{
    public IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    public ITenantContextAccessor TenantContextAccessor { get; } = tenantContextAccessor;
    public AxiomDbContextOptions<TContext> Options { get; } = options.Value;
    public IConnectionStringProvider ConnectionStringProvider { get; } = connectionStringProvider;

    protected IDbContextFactory<TContext> DbContextFactory { get; } = dbContextFactory;

    public virtual async ValueTask<TContext> GetAsync(CancellationToken cancellationToken = default)
    {
        //TODO: We might optimize here

        var unitOfWork = UnitOfWorkManager.RequiredCurrent;
        cancellationToken = cancellationToken.FallbackTo(unitOfWork.CancellationToken);

        var connectionString = await ConnectionStringProvider.FindAsync(Options.ConnectionStringName);
        var key = $"{typeof(TContext).FullName!}_{connectionString}";
        if (unitOfWork.Databases.TryGetValue(key, out var dbHandle))
        {
            return dbHandle.GetDatabase<TContext>();
        }

        var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            dbContext.Database.SetConnectionString(connectionString);    
        }

        dbHandle = await CreateHandleAsync(unitOfWork, dbContext, cancellationToken);
        unitOfWork.AddDatabase(key, dbHandle);

        return dbContext;
    }

    protected virtual async ValueTask<UnitOfWorkDatabaseHandle> CreateHandleAsync(
        IUnitOfWork unitOfWork,
        TContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (unitOfWork.Options.IsolationLevel.HasValue)
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(
                unitOfWork.Options.IsolationLevel.Value,
                cancellationToken);
            return new UnitOfWorkDatabaseHandle(
                dbContext,
                transaction,
                UnitOfWorkDatabaseHandleExtensions.SaveChangesAsync,
                UnitOfWorkDatabaseHandleExtensions.CommitAsync,
                UnitOfWorkDatabaseHandleExtensions.RollbackAsync);
        }

        if (unitOfWork.Options.TransactionBehavior == UnitOfWorkTransactionBehavior.Suppress)
        {
            return new UnitOfWorkDatabaseHandle(dbContext, UnitOfWorkDatabaseHandleExtensions.SaveChangesAsync);
        }

        return new UnitOfWorkDatabaseHandle(
            dbContext,
            UnitOfWorkDatabaseHandleExtensions.SaveChangesAsync,
            // When IsolationLevel exists it handled in first if condition
            UnitOfWorkDatabaseHandleExtensions.BeginTransactionAsync,
            UnitOfWorkDatabaseHandleExtensions.CommitAsync,
            UnitOfWorkDatabaseHandleExtensions.RollbackAsync);
    }
}