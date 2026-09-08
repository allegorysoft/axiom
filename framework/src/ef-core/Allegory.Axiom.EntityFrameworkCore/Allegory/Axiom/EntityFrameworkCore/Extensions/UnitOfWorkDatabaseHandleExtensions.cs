using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Allegory.Axiom.EntityFrameworkCore;

public static class UnitOfWorkDatabaseHandleExtensions
{
    public static Task SaveChangesAsync(
        UnitOfWorkDatabaseHandle dbHandle,
        CancellationToken cancellationToken = default)
    {
        return dbHandle.GetDatabase<DbContext>().SaveChangesAsync(cancellationToken);
    }

    public static async Task<object> BeginTransactionAsync(
        UnitOfWorkDatabaseHandle dbHandle,
        CancellationToken cancellationToken = default)
    {
        return await dbHandle.GetDatabase<DbContext>().Database.BeginTransactionAsync(cancellationToken);
    }

    public static Task CommitAsync(
        UnitOfWorkDatabaseHandle dbHandle,
        CancellationToken cancellationToken = default)
    {
        return dbHandle.GetTransaction<IDbContextTransaction>().CommitAsync(cancellationToken);
    }

    public static Task RollbackAsync(
        UnitOfWorkDatabaseHandle dbHandle,
        CancellationToken cancellationToken = default)
    {
        return dbHandle.GetTransaction<IDbContextTransaction>().RollbackAsync(cancellationToken);
    }
}