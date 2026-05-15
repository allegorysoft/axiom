namespace Allegory.Axiom.UnitOfWork;

public interface IUnitOfWorkManager
{
    IUnitOfWork? Current { get; }
    IUnitOfWork Begin(UnitOfWorkOptions? options = null);
}