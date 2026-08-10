using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Guid Id { get; }
    IUnitOfWork? Parent { get; }
    Activity? Activity { get; }
    UnitOfWorkOptions Options { get; }
    Dictionary<string, object> Items { get; }
    IReadOnlyDictionary<string, UnitOfWorkDatabaseHandle> Databases { get; }
    UnitOfWorkState State { get; }
    IServiceProvider ServiceProvider { get; }
    CancellationToken CancellationToken { get; }

    void AddDatabase(string key, UnitOfWorkDatabaseHandle handle);

    void AddHook(
        UnitOfWorkHookPoint hook,
        Func<Task> handler,
        UnitOfWorkHookPriority priority = UnitOfWorkHookPriority.Normal);

    /// <summary>
    /// Persists the pending changes tracked by this unit of work.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to observe for this operation. If provided, it is used instead of
    /// the <see cref="CancellationToken"/> the unit of work was created with.
    /// </param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes the unit of work, committing all tracked changes.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to observe for this operation. If provided, it is used instead of
    /// the <see cref="CancellationToken"/> the unit of work was created with.
    /// </param>
    Task CompleteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the unit of work, discarding all tracked changes.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to observe for this operation. Unlike <see cref="SaveChangesAsync"/> and
    /// <see cref="CompleteAsync"/>, this does not fall back to the unit of work's own
    /// <see cref="CancellationToken"/>, since rollback should be allowed to run even if
    /// that token has already been canceled.
    /// </param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}