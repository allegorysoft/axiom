using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

internal class UnitOfWork : IUnitOfWork
{
    // Implement async dispose pattern
    // What's the flow ? Save -> Commit/Rollback

    public Guid Id { get; } = Guid.NewGuid();
    public Activity? Activity { get; set; }
    public IUnitOfWork? Parent { get; set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Activity?.Dispose();
        UnitOfWorkManager.CurrentUnitOfWork.Value = Parent;
    }
}