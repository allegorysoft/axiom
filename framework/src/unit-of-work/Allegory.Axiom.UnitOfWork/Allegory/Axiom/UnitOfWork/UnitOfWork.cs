namespace Allegory.Axiom.UnitOfWork;

internal class UnitOfWork(
    UnitOfWorkOptions options,
    IUnitOfWork? parent = null)
    : UnitOfWorkBase(options, parent)
{
    // What's the flow ? Save -> Commit/Rollback

    public override void Dispose()
    {
        base.Dispose();
        Activity?.Dispose();
    }
}