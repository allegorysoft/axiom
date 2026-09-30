using System;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.DistributedLocking;

public abstract class DistributedLockBase(
    IOptions<DistributedLockOptions> options,
    IUnitOfWorkManager unitOfWorkManager) :
    IDistributedLock, ISingletonService
{
    protected DistributedLockOptions Options { get; } = options.Value;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    public ValueTask<IAsyncDisposable?> TryAcquireAsync(
        string key,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default)
    {
        key = NormalizeKey(key);
        var unitOfWork = UnitOfWorkManager.Current;
        if (unitOfWork != null)
        {
            cancellationToken = cancellationToken.FallbackTo(unitOfWork.CancellationToken);
        }

        return TryAcquireCoreAsync(key, timeout, cancellationToken);
    }

    protected abstract ValueTask<IAsyncDisposable?> TryAcquireCoreAsync(
        string key,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default);

    protected virtual string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(Options.KeyPrefix))
        {
            return key;
        }

        return Options.KeyPrefix + key;
    }
}