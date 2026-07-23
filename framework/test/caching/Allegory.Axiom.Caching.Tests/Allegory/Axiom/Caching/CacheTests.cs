using System;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.MultiTenancy;
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

    [Fact]
    public void ShouldApplyKeyPrefix()
    {
        const string prefix = "app:";

        var cache = new TestableCache(
            fixture.Service<HybridCache>(),
            Options.Create(new CacheOptions {KeyPrefix = prefix}),
            fixture.Service<ITenantContextAccessor>());

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
        using (fixture.Service<ITenantContextAccessor>().Change(Tenant))
        {
            Cache.Normalize<SomeCacheItem>("abc")
                .ShouldBe($"tenant:{Tenant.Id:D}:allegory:axiom:caching:some:abc");
        }
    }

    [Fact]
    public void ShouldIgnoreTenantForTenantAgnosticType()
    {
        using (fixture.Service<ITenantContextAccessor>().Change(Tenant))
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
}

[Dependency(SelfRegister = true)]
public class TestableCache(
    HybridCache hybridCache,
    IOptions<CacheOptions> options,
    ITenantContextAccessor accessor)
    : Cache(hybridCache, options, accessor)
{
    public string Normalize<T>(string key) =>
        NormalizeKey(key, CacheTypeDescriptors.GetOrAdd(typeof(T), GetCacheTypeDescriptor, Options));
}

public class SomeCacheItem;

[TenantAgnostic]
public class AgnosticCacheItem;

[CacheName("Custom.Name")]
public class NamedCacheItem;