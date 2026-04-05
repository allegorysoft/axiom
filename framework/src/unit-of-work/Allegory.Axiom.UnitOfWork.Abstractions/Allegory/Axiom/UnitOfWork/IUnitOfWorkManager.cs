using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.UnitOfWork;

public interface IUnitOfWorkManager : ISingletonService
{
    static abstract IUnitOfWork? Current { get; }
    IUnitOfWork Begin();
}