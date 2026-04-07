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
    public IReadOnlyDictionary<string, UnitOfWorkDatabaseHandle> Databases => Parent.Databases;
    public UnitOfWorkState State => Parent.State;

    public void AddDatabase(string key, UnitOfWorkDatabaseHandle handle) => Parent.AddDatabase(key, handle);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Parent.SaveChangesAsync(cancellationToken);

    public Task CompleteAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RollbackAsync(CancellationToken cancellationToken = default) => Parent.RollbackAsync(cancellationToken);

    public void Dispose()
    {
        UnitOfWorkManager.CurrentUnitOfWork.Value = Parent;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}