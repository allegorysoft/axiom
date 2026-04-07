using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

internal sealed class ChildUnitOfWork(IUnitOfWork parent) : IUnitOfWork
{
    public Guid Id { get; } = Guid.NewGuid();
    public IUnitOfWork Parent { get; } = parent;
    public Activity? Activity => Parent.Activity;
    public UnitOfWorkOptions Options => Parent.Options;
    public Dictionary<string, object> Items => Parent.Items;

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await Parent.SaveChangesAsync(cancellationToken);
    }

    public Task CompleteAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await Parent.RollbackAsync(cancellationToken);
    }

    public void Dispose()
    {
        UnitOfWorkManager.CurrentUnitOfWork.Value = Parent;
    }
}