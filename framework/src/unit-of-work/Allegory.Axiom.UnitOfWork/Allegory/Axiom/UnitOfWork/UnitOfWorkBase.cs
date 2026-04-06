using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

internal abstract class UnitOfWorkBase(UnitOfWorkOptions options) : IUnitOfWork
{
    // Implement async dispose pattern

    public Guid Id { get; } = Guid.NewGuid();
    public Activity? Activity { get; set; }
    public IUnitOfWork? Parent { get; set; }
    public UnitOfWorkOptions Options { get; } = options;

    public virtual Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task CompleteAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual void Dispose()
    {
        Activity?.Dispose();
        UnitOfWorkManager.CurrentUnitOfWork.Value = Parent;
    }
}