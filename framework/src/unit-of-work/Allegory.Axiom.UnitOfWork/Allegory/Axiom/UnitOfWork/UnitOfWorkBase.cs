using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

internal abstract class UnitOfWorkBase(
    UnitOfWorkOptions options,
    IUnitOfWork? parent = null)
    : IUnitOfWork
{
    // Implement async dispose pattern

    public Guid Id { get; } = Guid.NewGuid();
    public IUnitOfWork? Parent { get; } = parent;
    public virtual Activity? Activity { get; set; }
    public virtual UnitOfWorkOptions Options { get; } = options;
    public virtual Dictionary<string, object> Items { get; } = new();

    public virtual Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task CompleteAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual void Dispose()
    {
        UnitOfWorkManager.CurrentUnitOfWork.Value = Parent;
    }
}