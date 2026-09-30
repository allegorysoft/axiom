using System;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.UnitOfWork;
using Medallion.Threading;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.DistributedLocking;

public class MadelsonDistributedLock(
    IOptions<DistributedLockOptions> options,
    IUnitOfWorkManager unitOfWorkManager,
    IDistributedLockProvider provider) :
    DistributedLockBase(options, unitOfWorkManager)
{
    public IDistributedLockProvider Provider { get; } = provider;

    protected override async ValueTask<IAsyncDisposable?> TryAcquireCoreAsync(
        string key,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default)
    {
        var handle = await Provider.TryAcquireLockAsync(key, timeout, cancellationToken);
        return handle;
    }
}