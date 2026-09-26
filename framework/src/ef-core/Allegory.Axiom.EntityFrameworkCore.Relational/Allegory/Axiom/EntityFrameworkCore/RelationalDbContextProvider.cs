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
    IConnectionStringProvider connectionStringProvider) :
    DbContextProvider<TContext>(dbContextFactory, unitOfWorkManager, options, connectionStringProvider),
    ISingletonService
    where TContext : DbContext
{
    protected override async ValueTask<TContext> CreateDbContextAsync(
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

    protected override async Task TryBeginTransactionAsync(
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