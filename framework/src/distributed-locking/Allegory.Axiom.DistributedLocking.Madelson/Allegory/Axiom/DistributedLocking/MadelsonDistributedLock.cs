using Medallion.Threading;

namespace Allegory.Axiom.DistributedLocking;

public class MadelsonDistributedLock(IDistributedLockProvider provider) : DistributedLockBase
{
    public IDistributedLockProvider Provider { get; } = provider;
}