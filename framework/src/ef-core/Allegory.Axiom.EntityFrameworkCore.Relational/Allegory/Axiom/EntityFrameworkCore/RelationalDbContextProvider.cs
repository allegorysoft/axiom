using System;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Data.ConnectionStrings;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        if (unitOfWork.DbHandles.TryGetValue(key, out var dbHandle))
        {
            return ((EfCoreUnitOfWorkDbHandle<TContext>) dbHandle).Handle;
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
        EfCoreUnitOfWorkDbHandle<TContext> handle;

        if (unitOfWork.Options.IsolationLevel.HasValue)
        {
            await TryBeginTransactionAsync(unitOfWork, dbContext, cancellationToken);
            handle = new EfCoreUnitOfWorkDbHandle<TContext>(dbContext);
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

    protected virtual async Task TryBeginTransactionAsync(
        IUnitOfWork unitOfWork,
        TContext dbContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Database.BeginTransactionAsync(unitOfWork.Options.IsolationLevel!.Value, cancellationToken);
        }
        catch (NotSupportedException e)
        {
            var logger = unitOfWork.ServiceProvider.GetRequiredService<ILogger<UnitOfWorkDbHandle>>();
            logger.LogWarning(e, "Transaction not supported for {DbContext}", typeof(TContext));
        }
    }
}