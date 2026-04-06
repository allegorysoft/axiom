using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    Guid Id { get; }
    IUnitOfWork? Parent { get; }
    Activity? Activity { get; }
    UnitOfWorkOptions Options { get; }

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task CompleteAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}