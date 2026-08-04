using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.EntityFrameworkCore.Infrastructure;

namespace Allegory.Axiom.EntityFrameworkCore;

public class MongoDbContextProvider<TContext>(
    IDbContextFactory<TContext> dbContextFactory,
    IUnitOfWorkManager unitOfWorkManager)
    : DbContextProvider<TContext>(dbContextFactory, unitOfWorkManager)
    where TContext : DbContext
{
    public override async ValueTask<TContext> GetAsync(CancellationToken cancellationToken = default)
    {
        var unitOfWork = UnitOfWorkManager.RequiredCurrent;
        cancellationToken = cancellationToken.FallbackTo(unitOfWork.CancellationToken);
        var key = typeof(TContext).FullName!;

        if (unitOfWork.Databases.TryGetValue(key, out var dbHandle))
        {
            return dbHandle.GetDatabase<TContext>();
        }

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
}