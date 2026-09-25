using System;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

public abstract class UnitOfWorkDbHandle(object handle) : IDisposable, IAsyncDisposable
{
    public IUnitOfWork UnitOfWork { get; protected internal set; } = null!;

    public abstract Task SaveChangesAsync(CancellationToken cancellationToken = default);
    public abstract Task CommitAsync(CancellationToken cancellationToken = default);
    public abstract Task RollbackAsync(CancellationToken cancellationToken = default);

    public virtual void Dispose()
    {
        switch (handle)
        {
            case IDisposable disposable:
                disposable.Dispose();
                break;
            case IAsyncDisposable asyncDisposable:
                asyncDisposable.DisposeAsync().GetAwaiter().GetResult();
                break;
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        switch (handle)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }
}

public abstract class UnitOfWorkDbHandle<TDatabase>(
    TDatabase handle)
    : UnitOfWorkDbHandle(handle)
    where TDatabase : notnull
{
    public TDatabase Handle { get; } = handle;
}