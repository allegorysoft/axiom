using System;
using System.Threading;

namespace Allegory.Axiom.UnitOfWork;

public interface IUnitOfWorkManager
{
    IUnitOfWork? Current { get; }
    IUnitOfWork RequiredCurrent { get; }
    IUnitOfWork Begin(
        UnitOfWorkOptions? options = null,
        IServiceProvider? serviceProvider = null,
        CancellationToken cancellationToken = default);
}