using System;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Caching;

public class CacheTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected static readonly TenantContext Tenant =
        new(Guid.Parse("11111111-2222-3333-4444-555555555555"), "acme", "ACME");

    protected MockCache Cache { get; } = fixture.Service<MockCache>();
    protected ITenantContextAccessor TenantContextAccessor { get; } = fixture.Service<ITenantContextAccessor>();
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = fixture.Service<IUnitOfWorkManager>();

    // Set

    [Fact]
    public async Task ShouldSetImmediatelyWhenNoUnitOfWork()
    {
        await Cache.SetAsync(
            "mm-no-uow",
            new SomeCacheItem(),
            mutationMode: CacheMutationMode.Immediate,
            cancellationToken: TestContext.Current.CancellationToken);
        (await Cache.ExistsAsync<SomeCacheItem>("mm-no-uow")).ShouldBeTrue();

        await Cache.SetAsync(
            "mm-fallback",
            new SomeCacheItem(),
            mutationMode: CacheMutationMode.OnUnitOfWorkComplete,
            cancellationToken: TestContext.Current.CancellationToken);
        (await Cache.ExistsAsync<SomeCacheItem>("mm-fallback")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldSetImmediatelyWhenModeIsImmediateInsideUnitOfWork()
    {
        await using var unitOfWork = UnitOfWorkManager.Begin(cancellationToken: TestContext.Current.CancellationToken);

        await Cache.SetAsync(
            "mm-immediate",
            new SomeCacheItem(),
            mutationMode: CacheMutationMode.Immediate,
            cancellationToken: TestContext.Current.CancellationToken);

        (await Cache.ExistsAsync<SomeCacheItem>("mm-immediate")).ShouldBeTrue();

        await unitOfWork.CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ShouldDeferSetUntilUnitOfWorkCompletes()
    {
        await using (var unitOfWork = UnitOfWorkManager.Begin(cancellationToken: TestContext.Current.CancellationToken))
        {
            await Cache.SetAsync(
                "mm-deferred-set",
                new SomeCacheItem(),
                mutationMode: CacheMutationMode.OnUnitOfWorkComplete,
                cancellationToken: TestContext.Current.CancellationToken);

            (await Cache.ExistsAsync<SomeCacheItem>("mm-deferred-set")).ShouldBeFalse();

            await unitOfWork.CompleteAsync(CancellationToken.None);
        }

        (await Cache.ExistsAsync<SomeCacheItem>("mm-deferred-set")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotSetWhenUnitOfWorkRollsBack()
    {
        await using (UnitOfWorkManager.Begin(cancellationToken: TestContext.Current.CancellationToken))
        {
            await Cache.SetAsync(
                "mm-rollback-set",
                new SomeCacheItem(),
                mutationMode: CacheMutationMode.OnUnitOfWorkComplete,
                cancellationToken: TestContext.Current.CancellationToken);
        }

        (await Cache.ExistsAsync<SomeCacheItem>("mm-rollback-set")).ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldCaptureTenantAtCallTimeForDeferredSet()
    {
        await using (var unitOfWork = UnitOfWorkManager.Begin(cancellationToken: TestContext.Current.CancellationToken))
        {
            using (TenantContextAccessor.Change(Tenant))
            {
                await Cache.SetAsync(
                    "mm-tenant",
                    new TenantOwnedCacheItem(),
                    mutationMode: CacheMutationMode.OnUnitOfWorkComplete,
                    cancellationToken: TestContext.Current.CancellationToken);

                (await Cache.ExistsAsync<TenantOwnedCacheItem>("mm-tenant")).ShouldBeFalse();
            }

            await unitOfWork.CompleteAsync(CancellationToken.None);
        }

        using (TenantContextAccessor.Change(Tenant))
        {
            (await Cache.ExistsAsync<TenantOwnedCacheItem>("mm-tenant")).ShouldBeTrue();
        }

        (await Cache.ExistsAsync<TenantOwnedCacheItem>("mm-tenant")).ShouldBeFalse();
    }

    // Remove

    [Fact]
    public async Task ShouldRemoveImmediatelyWhenNoUnitOfWork()
    {
        await Cache.SetAsync(
            "mm-remove-no-uow",
            new SomeCacheItem(),
            cancellationToken: TestContext.Current.CancellationToken);
        await Cache.RemoveAsync<SomeCacheItem>(
            "mm-remove-no-uow",
            CacheMutationMode.Immediate,
            TestContext.Current.CancellationToken);
        (await Cache.ExistsAsync<SomeCacheItem>("mm-remove-no-uow")).ShouldBeFalse();

        await Cache.SetAsync(
            "mm-remove-fallback",
            new SomeCacheItem(),
            cancellationToken: TestContext.Current.CancellationToken);
        await Cache.RemoveAsync<SomeCacheItem>(
            "mm-remove-fallback",
            CacheMutationMode.OnUnitOfWorkComplete,
            TestContext.Current.CancellationToken);
        (await Cache.ExistsAsync<SomeCacheItem>("mm-remove-fallback")).ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldRemoveImmediatelyWhenModeIsImmediateInsideUnitOfWork()
    {
        await Cache.SetAsync(
            "mm-remove-immediate",
            new SomeCacheItem(),
            cancellationToken: TestContext.Current.CancellationToken);

        await using var unitOfWork = UnitOfWorkManager.Begin(cancellationToken: TestContext.Current.CancellationToken);

        await Cache.RemoveAsync<SomeCacheItem>(
            "mm-remove-immediate",
            CacheMutationMode.Immediate,
            TestContext.Current.CancellationToken);
        (await Cache.ExistsAsync<SomeCacheItem>("mm-remove-immediate")).ShouldBeFalse();

        await unitOfWork.CompleteAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ShouldDeferRemoveUntilUnitOfWorkCompletes()
    {
        await Cache.SetAsync(
            "mm-deferred-remove",
            new SomeCacheItem(),
            cancellationToken: TestContext.Current.CancellationToken);

        await using (var unitOfWork = UnitOfWorkManager.Begin(cancellationToken: TestContext.Current.CancellationToken))
        {
            await Cache.RemoveAsync<SomeCacheItem>(
                "mm-deferred-remove",
                CacheMutationMode.OnUnitOfWorkComplete,
                TestContext.Current.CancellationToken);

            (await Cache.ExistsAsync<SomeCacheItem>("mm-deferred-remove")).ShouldBeTrue();

            await unitOfWork.CompleteAsync(CancellationToken.None);
        }

        (await Cache.ExistsAsync<SomeCacheItem>("mm-deferred-remove")).ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldNotRemoveWhenUnitOfWorkRollsBack()
    {
        await Cache.SetAsync(
            "mm-rollback-remove",
            new SomeCacheItem(),
            cancellationToken: TestContext.Current.CancellationToken);

        await using (UnitOfWorkManager.Begin(cancellationToken: TestContext.Current.CancellationToken))
        {
            await Cache.RemoveAsync<SomeCacheItem>(
                "mm-rollback-remove",
                CacheMutationMode.OnUnitOfWorkComplete,
                TestContext.Current.CancellationToken);
        }

        (await Cache.ExistsAsync<SomeCacheItem>("mm-rollback-remove")).ShouldBeTrue();
    }

    // RemoveMany

    [Fact]
    public async Task ShouldDeferRemoveManyUntilUnitOfWorkCompletes()
    {
        await Cache.SetAsync(
            "mm-many-1",
            new SomeCacheItem(),
            cancellationToken: TestContext.Current.CancellationToken);
        await Cache.SetAsync(
            "mm-many-2",
            new SomeCacheItem(),
            cancellationToken: TestContext.Current.CancellationToken);

        await using (var unitOfWork = UnitOfWorkManager.Begin(cancellationToken: TestContext.Current.CancellationToken))
        {
            await Cache.RemoveAsync<SomeCacheItem>(
                ["mm-many-1", "mm-many-2"],
                CacheMutationMode.OnUnitOfWorkComplete,
                TestContext.Current.CancellationToken);

            (await Cache.ExistsAsync<SomeCacheItem>("mm-many-1")).ShouldBeTrue();
            (await Cache.ExistsAsync<SomeCacheItem>("mm-many-2")).ShouldBeTrue();

            await unitOfWork.CompleteAsync(CancellationToken.None);
        }

        (await Cache.ExistsAsync<SomeCacheItem>("mm-many-1")).ShouldBeFalse();
        (await Cache.ExistsAsync<SomeCacheItem>("mm-many-2")).ShouldBeFalse();
    }

    // RemoveByTag

    [Fact]
    public async Task ShouldDeferRemoveByTagUntilUnitOfWorkCompletes()
    {
        await Cache.SetAsync(
            "mm-tagged",
            new SomeCacheItem(),
            tags: ["mm-tag"],
            cancellationToken: TestContext.Current.CancellationToken);

        await using (var unitOfWork = UnitOfWorkManager.Begin(cancellationToken: TestContext.Current.CancellationToken))
        {
            await Cache.RemoveByTagAsync(
                "mm-tag",
                CacheMutationMode.OnUnitOfWorkComplete,
                TestContext.Current.CancellationToken);

            (await Cache.ExistsAsync<SomeCacheItem>("mm-tagged")).ShouldBeTrue();

            await unitOfWork.CompleteAsync(CancellationToken.None);
        }

        (await Cache.ExistsAsync<SomeCacheItem>("mm-tagged")).ShouldBeFalse();
    }

    // RemoveByTag (many)

    [Fact]
    public async Task ShouldDeferRemoveByTagsUntilUnitOfWorkCompletes()
    {
        await Cache.SetAsync(
            "mm-multi-tagged-1",
            new SomeCacheItem(),
            tags: ["mm-tag-a"],
            cancellationToken: TestContext.Current.CancellationToken);
        await Cache.SetAsync(
            "mm-multi-tagged-2",
            new SomeCacheItem(),
            tags: ["mm-tag-b"],
            cancellationToken: TestContext.Current.CancellationToken);

        await using (var unitOfWork = UnitOfWorkManager.Begin(cancellationToken: TestContext.Current.CancellationToken))
        {
            await Cache.RemoveByTagAsync(
                ["mm-tag-a", "mm-tag-b"],
                CacheMutationMode.OnUnitOfWorkComplete,
                TestContext.Current.CancellationToken);

            (await Cache.ExistsAsync<SomeCacheItem>("mm-multi-tagged-1")).ShouldBeTrue();
            (await Cache.ExistsAsync<SomeCacheItem>("mm-multi-tagged-2")).ShouldBeTrue();

            await unitOfWork.CompleteAsync(CancellationToken.None);
        }

        (await Cache.ExistsAsync<SomeCacheItem>("mm-multi-tagged-1")).ShouldBeFalse();
        (await Cache.ExistsAsync<SomeCacheItem>("mm-multi-tagged-2")).ShouldBeFalse();
    }
}

[Dependency(SelfRegister = true)]
public class MockCache(
    HybridCache hybridCache,
    ICacheKeyNormalizer keyNormalizer,
    IUnitOfWorkManager uowManager) : 
    Cache(hybridCache, keyNormalizer, uowManager)
{
    public async ValueTask<bool> ExistsAsync<T>(string key) where T : class
    {
        var hit = true;

        await HybridCache.GetOrCreateAsync(
            KeyNormalizer.Normalize<T>(key),
            _ =>
            {
                hit = false;
                return ValueTask.FromResult<T?>(null);
            },
            new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableLocalCacheWrite | HybridCacheEntryFlags.DisableDistributedCacheWrite
            });

        return hit;
    }
}
