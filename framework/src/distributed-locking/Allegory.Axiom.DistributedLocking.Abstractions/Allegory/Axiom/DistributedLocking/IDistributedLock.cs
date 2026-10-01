using System;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.DistributedLocking;

public interface IDistributedLock
{
    ValueTask<IAsyncDisposable?> TryAcquireAsync(
        string key,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default);
}