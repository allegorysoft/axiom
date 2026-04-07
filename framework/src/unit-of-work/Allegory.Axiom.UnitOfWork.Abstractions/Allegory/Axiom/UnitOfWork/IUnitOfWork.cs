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

    void AddDatabase(string key, UnitOfWorkDatabaseHandle handle);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task CompleteAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}