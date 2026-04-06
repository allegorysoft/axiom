namespace Allegory.Axiom.UnitOfWork;

internal class UnitOfWork(UnitOfWorkOptions options) : UnitOfWorkBase(options)
{
    // What's the flow ? Save -> Commit/Rollback
}