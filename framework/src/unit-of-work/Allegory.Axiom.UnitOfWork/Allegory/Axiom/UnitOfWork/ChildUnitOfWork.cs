using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

internal class ChildUnitOfWork(IUnitOfWork parent,
    UnitOfWorkOptions options) : UnitOfWorkBase(options)
{
    public override IUnitOfWork? Parent { get; set; } = parent;
    public override Activity? Activity => Parent!.Activity;
    public override UnitOfWorkOptions Options => Parent!.Options;
    public override Dictionary<string, object> Items => Parent!.Items;

    public override async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await Parent!.SaveChangesAsync(cancellationToken);
    }

    public override async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await Parent!.RollbackAsync(cancellationToken);
    }
}