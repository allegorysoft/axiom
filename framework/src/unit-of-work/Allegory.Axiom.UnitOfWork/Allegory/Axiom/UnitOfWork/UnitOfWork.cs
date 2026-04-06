namespace Allegory.Axiom.UnitOfWork;

internal class UnitOfWork(UnitOfWorkOptions options) : UnitOfWorkBase(options)
{
    // What's the flow ? Save -> Commit/Rollback

    public override void Dispose()
    {
        base.Dispose();
        Activity?.Dispose();
    }
}