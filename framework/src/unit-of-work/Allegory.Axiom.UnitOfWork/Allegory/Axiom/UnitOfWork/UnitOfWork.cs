using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

internal sealed class UnitOfWork(UnitOfWorkOptions options) : IUnitOfWork
{
    // Implement async dispose pattern
    // What's the flow ? Save -> Commit/Rollback

    public Guid Id { get; } = Guid.NewGuid();
    public IUnitOfWork? Parent { get; set; }
    public Activity? Activity { get; set; }
    public UnitOfWorkOptions Options { get; } = options;
    public Dictionary<string, object> Items { get; } = new();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task CompleteAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Dispose()
    {
        Activity?.Dispose();
        UnitOfWorkManager.CurrentUnitOfWork.Value = Parent;
    }
}