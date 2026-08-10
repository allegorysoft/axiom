using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

[Dependency(AutoRegister = false)]
public class MongoDbContextProvider<TContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IUnitOfWorkManager unitOfWorkManager)
    : IDbContextProvider<TContext>
    where TContext : DbContext
{
    public IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected IDbContextFactory<TContext> DbContextFactory { get; } = dbContextFactory;

    public async ValueTask<TContext> GetAsync(CancellationToken cancellationToken = default)
    {
        var unitOfWork = UnitOfWorkManager.RequiredCurrent;
        cancellationToken = cancellationToken.FallbackTo(unitOfWork.CancellationToken);
        var key = typeof(TContext).FullName!;

        if (unitOfWork.Databases.TryGetValue(key, out var dbHandle))
        {
            return dbHandle.GetDatabase<TContext>();
        }

        // We can use existing configurations like this
        // var options = Services.GetRequiredService<DbContextOptions<TContext>>();
        // var builder = new DbContextOptionsBuilder<TContext>(options); 

        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        // optionsBuilder.UseMongoDB(connectionString)
        //  Registering IMongoClient as a singleton and passing it into UseMongoDB is the recommended pattern;
        //  - We might use client factory and gave the client just like RabbitMqConnectionFactory
        // Create DbContext instance with ActivatorUtilities instead DbContextFactory

        var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        if (unitOfWork.Options.IsolationLevel.HasValue)
        {
            var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
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
                UnitOfWorkDatabaseHandleExtensions.BeginTransactionAsync,
                UnitOfWorkDatabaseHandleExtensions.CommitAsync,
                UnitOfWorkDatabaseHandleExtensions.RollbackAsync);
        }

        unitOfWork.AddDatabase(key, dbHandle);
        return dbContext;
    }
}