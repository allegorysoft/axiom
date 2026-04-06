using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

internal class ChildUnitOfWork(UnitOfWorkOptions options) : UnitOfWorkBase(options)
{
    public override async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await Parent!.SaveChangesAsync(cancellationToken);
    }

    public override async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await Parent!.RollbackAsync(cancellationToken);
    }
}