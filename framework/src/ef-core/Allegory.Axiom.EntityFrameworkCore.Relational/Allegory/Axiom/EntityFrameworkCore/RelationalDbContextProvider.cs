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
    IConnectionStringProvider connectionStringProvider,
    ITenantContextAccessor tenantContextAccessor,
    IOptions<AxiomDbContextOptions<TContext>> options)
    : IDbContextProvider<TContext>, ISingletonService
    where TContext : DbContext
{
    public IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    public ITenantContextAccessor TenantContextAccessor { get; } = tenantContextAccessor;
    public AxiomDbContextOptions<TContext> Options { get; } = options.Value;
    protected IDbContextFactory<TContext> DbContextFactory { get; } = dbContextFactory;
    protected IConnectionStringProvider ConnectionStringProvider { get; } = connectionStringProvider;

    public async ValueTask<TContext> GetAsync(CancellationToken cancellationToken = default)
    {
        var unitOfWork = UnitOfWorkManager.RequiredCurrent;
        cancellationToken = cancellationToken.FallbackTo(unitOfWork.CancellationToken);

        var itemKey = $"db_{TenantContextAccessor.Current?.Id.ToString() ?? "host"}_{typeof(TContext).FullName!}";
        if (unitOfWork.Items.TryGetValue(itemKey, out var context))
        {
            return (TContext) context;
        }

        var connectionString = await ConnectionStringProvider.GetAsync(Options.ConnectionStringName);
        var key = $"{typeof(TContext).FullName!}_{connectionString}";
        if (unitOfWork.Databases.TryGetValue(key, out var dbHandle))
        {
            return dbHandle.GetDatabase<TContext>();
        }

        var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Database.SetConnectionString(connectionString);
        unitOfWork.Items.Add(itemKey, dbContext);

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