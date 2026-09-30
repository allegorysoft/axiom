using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.DistributedLocking;

public class InProcessDistributedLock(
    IOptions<DistributedLockOptions> options,
    IUnitOfWorkManager unitOfWorkManager) :
    DistributedLockBase(options, unitOfWorkManager)
{
    protected ConcurrentDictionary<string, LockEntry> Locks { get; } = new();

    protected override async ValueTask<IAsyncDisposable?> TryAcquireCoreAsync(
        string key,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default)
    {
        var entry = GetLockEntry(key);

        try
        {
            if (!await entry.Semaphore.WaitAsync(timeout, cancellationToken))
            {
                ReleaseReference(key, entry);
                return null;
            }
        }
        catch
        {
            ReleaseReference(key, entry);
            throw;
        }

        return new LockHandle(this, key, entry);
    }

    protected virtual LockEntry GetLockEntry(string key)
    {
        while (true)
        {
            var entry = Locks.GetOrAdd(
                key,
                static _ => new LockEntry());

            if (entry.TryAddReference())
            {
                return entry;
            }

            RemoveEntry(key, entry);
        }
    }

    protected virtual void Release(string key, LockEntry entry)
    {
        entry.Semaphore.Release();
        ReleaseReference(key, entry);
    }

    protected virtual void ReleaseReference(string key, LockEntry entry)
    {
        if (entry.ReleaseReference())
        {
            RemoveEntry(key, entry);
        }
    }

    protected virtual void RemoveEntry(string key, LockEntry entry)
    {
        if (Locks.TryRemove(new KeyValuePair<string, LockEntry>(key, entry)))
        {
            entry.Dispose();
        }
    }

    protected class LockEntry : IDisposable
    {
        private readonly Lock _syncRoot = new();
        private int _references;
        private bool _retired;

        public SemaphoreSlim Semaphore { get; } = new(1, 1);

        public bool TryAddReference()
        {
            lock (_syncRoot)
            {
                if (_retired)
                {
                    return false;
                }

                _references++;
                return true;
            }
        }

        public bool ReleaseReference()
        {
            lock (_syncRoot)
            {
                _references--;

                if (_references != 0)
                {
                    return false;
                }

                _retired = true;
                return true;
            }
        }

        public void Dispose()
        {
            Semaphore.Dispose();
        }
    }

    private sealed class LockHandle(InProcessDistributedLock owner, string key, LockEntry entry) : IAsyncDisposable
    {
        private LockEntry? _entry = entry;

        public ValueTask DisposeAsync()
        {
            var current = Interlocked.Exchange(ref _entry, null);

            if (current != null)
            {
                owner.Release(key, current);
            }

            return ValueTask.CompletedTask;
        }
    }
}