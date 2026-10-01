using System;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.DistributedLocking;

public class InProcessDistributedLockTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected IDistributedLock DistributedLock => fixture.Service<IDistributedLock>();

    [Fact]
    public async Task ShouldAcquireLock()
    {
        await using var handle =
            await DistributedLock.TryAcquireAsync("key", cancellationToken: CancellationToken.None);

        handle.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldNotAcquireSameKeyWhenAlreadyAcquired()
    {
        await using var first =
            await DistributedLock.TryAcquireAsync("key", cancellationToken: CancellationToken.None);

        var second = await DistributedLock.TryAcquireAsync("key", cancellationToken: CancellationToken.None);

        first.ShouldNotBeNull();
        second.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldAcquireSameKeyAfterReleased()
    {
        await using (var first =
                     await DistributedLock.TryAcquireAsync("key", cancellationToken: CancellationToken.None))
        {
            first.ShouldNotBeNull();
        }

        await using var second =
            await DistributedLock.TryAcquireAsync("key", cancellationToken: CancellationToken.None);

        second.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldAcquireDifferentKeysIndependently()
    {
        await using var first =
            await DistributedLock.TryAcquireAsync("key-1", cancellationToken: CancellationToken.None);
        await using var second =
            await DistributedLock.TryAcquireAsync("key-2", cancellationToken: CancellationToken.None);

        first.ShouldNotBeNull();
        second.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldWaitForSameKeyUntilReleased()
    {
        var first = await DistributedLock.TryAcquireAsync("key", cancellationToken: CancellationToken.None);
        first.ShouldNotBeNull();

        var task = DistributedLock
            .TryAcquireAsync("key", TimeSpan.FromSeconds(5), cancellationToken: CancellationToken.None)
            .AsTask();

        await Task.Delay(100, cancellationToken: CancellationToken.None);

        task.IsCompleted.ShouldBeFalse();

        await first.DisposeAsync();

        await using var second = await task;

        second.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldReturnNullWhenTimeoutElapsed()
    {
        await using var first = await DistributedLock.TryAcquireAsync("key", cancellationToken: CancellationToken.None);
        first.ShouldNotBeNull();

        await using var second = await DistributedLock.TryAcquireAsync(
            "key",
            TimeSpan.FromMilliseconds(100),
            cancellationToken: CancellationToken.None);

        second.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldRespectCancellationToken()
    {
        await using var first = await DistributedLock.TryAcquireAsync("key", cancellationToken: CancellationToken.None);
        first.ShouldNotBeNull();

        using var cancellationTokenSource = new CancellationTokenSource();

        var task = DistributedLock.TryAcquireAsync(
                "key",
                TimeSpan.FromSeconds(5),
                cancellationTokenSource.Token)
            .AsTask();

        await cancellationTokenSource.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => task);
    }

    [Fact]
    public async Task ShouldAllowOnlyOneConcurrentAcquisitionForSameKey()
    {
        const int degree = 8;
        var handles = new IAsyncDisposable?[degree];

        await Parallel.ForAsync(0, degree,
            async (i, _) =>
            {
                handles[i] = await DistributedLock.TryAcquireAsync("key", cancellationToken: CancellationToken.None);
            });

        var acquiredCount = 0;

        foreach (var handle in handles)
        {
            if (handle != null)
            {
                acquiredCount++;
                await handle.DisposeAsync();
            }
        }

        acquiredCount.ShouldBe(1);
    }
}