using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Data.ConnectionStrings;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

    protected override Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        TContext dbContext,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }
}