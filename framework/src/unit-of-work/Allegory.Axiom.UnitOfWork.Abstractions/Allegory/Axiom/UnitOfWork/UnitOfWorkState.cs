namespace Allegory.Axiom.UnitOfWork;

public enum UnitOfWorkState
{
    Started,
    Committing,
    Committed,
    RollingBack,
    RolledBack,
    Disposed
}