using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Data.ConnectionStrings;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EntityFrameworkCore;

public abstract class DbContextProvider<TContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IUnitOfWorkManager unitOfWorkManager,
    IOptions<AxiomDbContextOptions<TContext>> options,
    IConnectionStringProvider connectionStringProvider) :
    IDbContextProvider<TContext> where TContext : DbContext
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

        if (TryGetDbContext(unitOfWork, connectionString, out var dbContext, out var key))
        {
            return dbContext!;
        }

        dbContext = await CreateDbContextAsync(unitOfWork, connectionString, cancellationToken);
        await AddDbHandleAsync(unitOfWork, key!, dbContext, cancellationToken);

        return dbContext;
    }

    protected virtual bool TryGetDbContext(
        IUnitOfWork unitOfWork,
        string? connectionString,
        out TContext? dbContext,
        out string? key)
    {
        key = $"{Options.ConnectionStringName}_{connectionString}"; //TODO: We might optimize here
        dbContext = null;

        if (unitOfWork.DbHandles.TryGetValue(key, out var dbHandle))
        {
            dbContext = ((EfCoreUnitOfWorkDbHandle<TContext>) dbHandle).Handle;
            return true;
        }

        return false;
    }

    protected abstract ValueTask<TContext> CreateDbContextAsync(
        IUnitOfWork unitOfWork,
        string? connectionString,
        CancellationToken cancellationToken = default);

    protected virtual async Task AddDbHandleAsync(
        IUnitOfWork unitOfWork,
        string key,
        TContext dbContext,
        CancellationToken cancellationToken = default)
    {
        EfCoreUnitOfWorkDbHandle<TContext> handle;

        if (unitOfWork.Options.IsolationLevel.HasValue)
        {
            IDbContextTransaction? transaction = null;
            try
            {
                transaction = await BeginTransactionAsync(
                    unitOfWork.Options.IsolationLevel!.Value, dbContext, cancellationToken);
            }
            catch (NotSupportedException e)
            {
                var logger = unitOfWork.ServiceProvider.GetRequiredService<ILogger<UnitOfWorkDbHandle>>();
                logger.LogWarning(e, "Transaction not supported for {DbContext}", typeof(TContext));
            }

            handle = new EfCoreUnitOfWorkDbHandle<TContext>(dbContext, transaction);
        }
        else if (unitOfWork.Options.TransactionBehavior == UnitOfWorkTransactionBehavior.Suppress)
        {
            handle = new EfCoreUnitOfWorkDbHandle<TContext>(dbContext);
        }
        else
        {
            handle = new EfCoreUnitOfWorkDbHandle<TContext>(dbContext, isLazyTransactional: true);
        }

        unitOfWork.AddDbHandle(key, handle);
    }

    protected abstract Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        TContext dbContext,
        CancellationToken cancellationToken = default);
}