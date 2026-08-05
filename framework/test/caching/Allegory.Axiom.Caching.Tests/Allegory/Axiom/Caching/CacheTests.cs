using System;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Caching;

public class CacheTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected static readonly TenantContext Tenant =
        new(Guid.Parse("11111111-2222-3333-4444-555555555555"), "acme", "ACME");

    protected TestableCache Cache { get; } = fixture.Service<TestableCache>();
    protected ITenantContextAccessor TenantContextAccessor { get; } = fixture.Service<ITenantContextAccessor>();
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = fixture.Service<IUnitOfWorkManager>();

    // Descriptor

    [Fact]
    public void ShouldDeriveContextNameFromType() =>
        Cache.Descriptor<SomeCacheItem>().Name.ShouldBe("allegory:axiom:caching:some");

    [Fact]
    public void ShouldUseCacheNameAttributeForContextName() =>
        Cache.Descriptor<NamedCacheItem>().Name.ShouldBe("custom:name");

    [Fact]
    public void ShouldMarkTenantAgnosticType() =>
        Cache.Descriptor<AgnosticCacheItem>().IsTenantAgnostic.ShouldBeTrue();

    [Fact]
    public void ShouldNotMarkTenantAgnosticByDefault() =>
        Cache.Descriptor<SomeCacheItem>().IsTenantAgnostic.ShouldBeFalse();

    [Fact]
    public void ShouldHaveNullEntryOptionsWhenUnconfigured() =>
        Cache.Descriptor<SomeCacheItem>().EntryOptions.ShouldBeNull();

    [Fact]
    public void ShouldBuildDescriptorByConfiguredCacheTypeOptions()
    {
        var entryOptions = new HybridCacheEntryOptions {Expiration = TimeSpan.FromMinutes(5)};

        var cache = new TestableCache(
            fixture.Service<HybridCache>(),
            Options.Create(new CacheOptions
            {
                Types = {[typeof(SomeCacheItem)] = new CacheTypeOptions
                {
                    Name = "custom-name",
                    IsTenantAgnostic = true,
                    EntryOptions = entryOptions
                }}
            }),
            fixture.Service<ITenantContextAccessor>(),
            fixture.Service<IUnitOfWorkManager>());

        var descriptor = cache.Descriptor<SomeCacheItem>();
        descriptor.Name.ShouldBe("custom-name");
        descriptor.IsTenantAgnostic.ShouldBeTrue();
        descriptor.EntryOptions.ShouldBeSameAs(entryOptions);
    }

    // Key normalization 

    [Fact]
    public void ShouldApplyKeyPrefix()
    {
        const string prefix = "app:";

        var cache = new TestableCache(
            fixture.Service<HybridCache>(),
            Options.Create(new CacheOptions {KeyPrefix = prefix}),
            fixture.Service<ITenantContextAccessor>(),
            fixture.Service<IUnitOfWorkManager>());

        cache.Normalize<SomeCacheItem>("abc").ShouldStartWith(prefix);
    }

    [Fact]
    public void ShouldNormalizeHostKey()
    {
        Cache.Normalize<SomeCacheItem>("abc").ShouldBe("allegory:axiom:caching:some:abc");
    }

    [Fact]
    public void ShouldNormalizeTenantKey()
    {
        var hostKey = Cache.Normalize<SomeCacheItem>("abc");
        string tenantKey;

        using (TenantContextAccessor.Change(Tenant))
        {
            tenantKey = Cache.Normalize<SomeCacheItem>("abc");
        }

        hostKey.ShouldBe("allegory:axiom:caching:some:abc");
        tenantKey.ShouldBe($"tenant:{Tenant.Id:D}:allegory:axiom:caching:some:abc");
    }

    [Fact]
    public void ShouldIgnoreTenantForTenantAgnosticType()
    {
        using (TenantContextAccessor.Change(Tenant))
        {
            Cache.Normalize<AgnosticCacheItem>("abc")
                .ShouldBe("allegory:axiom:caching:agnostic:abc");
        }
    }

    [Fact]
    public void ShouldUseCacheNameAttribute()
    {
        Cache.Normalize<NamedCacheItem>("abc").ShouldBe("custom:name:abc");
    }

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
                    new SomeCacheItem(),
                    mutationMode: CacheMutationMode.OnUnitOfWorkComplete,
                    cancellationToken: TestContext.Current.CancellationToken);

                (await Cache.ExistsAsync<SomeCacheItem>("mm-tenant")).ShouldBeFalse();
            }

            await unitOfWork.CompleteAsync(CancellationToken.None);
        }

        using (TenantContextAccessor.Change(Tenant))
        {
            (await Cache.ExistsAsync<SomeCacheItem>("mm-tenant")).ShouldBeTrue();
        }

        (await Cache.ExistsAsync<SomeCacheItem>("mm-tenant")).ShouldBeFalse();
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
public class TestableCache(
    HybridCache hybridCache,
    IOptions<CacheOptions> options,
    ITenantContextAccessor accessor,
    IUnitOfWorkManager uowManager)
    : Cache(hybridCache, options, accessor, uowManager)
{
    public string Normalize<T>(string key) =>
        NormalizeKey(key, CacheTypeDescriptors.GetOrAdd(typeof(T), GetCacheTypeDescriptor, Options));

    public async ValueTask<bool> ExistsAsync<T>(string key) where T : class
    {
        var hit = true;

        await HybridCache.GetOrCreateAsync(
            Normalize<T>(key),
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

    public CacheTypeDescriptor Descriptor<T>() => CacheTypeDescriptors.GetOrAdd(typeof(T), GetCacheTypeDescriptor, Options);
}

public class SomeCacheItem;

[TenantAgnostic]
public class AgnosticCacheItem;

[CacheName("Custom.Name")]
public class NamedCacheItem;