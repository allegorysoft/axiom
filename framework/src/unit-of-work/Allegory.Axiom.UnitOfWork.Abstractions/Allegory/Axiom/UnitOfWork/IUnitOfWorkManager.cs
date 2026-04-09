using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.UnitOfWork;

public interface IUnitOfWorkManager : ISingletonService
{
    IUnitOfWork? Current { get; }
    IUnitOfWork Begin(UnitOfWorkOptions? options = null);
}