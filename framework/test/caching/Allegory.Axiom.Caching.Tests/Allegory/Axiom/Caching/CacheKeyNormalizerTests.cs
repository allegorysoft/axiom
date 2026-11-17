using System;
using Allegory.Axiom.MultiTenancy;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Caching;

public class CacheKeyNormalizerTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected static readonly TenantContext Tenant =
        new(Guid.Parse("11111111-2222-3333-4444-555555555555"), "acme", "ACME");

    protected ICacheKeyNormalizer Normalizer => fixture.Service<ICacheKeyNormalizer>();
    protected ITenantContextAccessor TenantContextAccessor { get; } = fixture.Service<ITenantContextAccessor>();

    // Descriptor

    [Fact]
    public void ShouldDeriveContextNameFromType() =>
        Normalizer.GetDescriptor<SomeCacheItem>().Name.ShouldBe("allegory:axiom:caching:some");

    [Fact]
    public void ShouldUseCacheNameAttributeForContextName() =>
        Normalizer.GetDescriptor<NamedCacheItem>().Name.ShouldBe("custom:name");

    [Fact]
    public void ShouldMarkTenantOwnedType() =>
        Normalizer.GetDescriptor<TenantOwnedCacheItem>().IsTenantOwned.ShouldBeTrue();

    [Fact]
    public void ShouldNotMarkTenantOwnedByDefault() =>
        Normalizer.GetDescriptor<SomeCacheItem>().IsTenantOwned.ShouldBeFalse();

    [Fact]
    public void ShouldHaveNullEntryOptionsWhenUnconfigured() =>
        Normalizer.GetDescriptor<SomeCacheItem>().EntryOptions.ShouldBeNull();

    [Fact]
    public void ShouldBuildDescriptorByConfiguredCacheTypeOptions()
    {
        var entryOptions = new HybridCacheEntryOptions {Expiration = TimeSpan.FromMinutes(5)};

        var normalizer = new CacheKeyNormalizer(
            Options.Create(new CacheOptions
            {
                Types =
                {
                    [typeof(SomeCacheItem)] = new CacheTypeOptions
                    {
                        Name = "custom-name",
                        IsTenantOwned = true,
                        EntryOptions = entryOptions
                    }
                }
            }),
            fixture.Service<ITenantContextAccessor>());

        var descriptor = normalizer.GetDescriptor<SomeCacheItem>();
        descriptor.Name.ShouldBe("custom-name");
        descriptor.IsTenantOwned.ShouldBeTrue();
        descriptor.EntryOptions.ShouldBeSameAs(entryOptions);
    }

    // Key normalization 

    [Fact]
    public void ShouldApplyKeyPrefix()
    {
        const string prefix = "app:";

        var normalizer = new CacheKeyNormalizer(
            Options.Create(new CacheOptions {KeyPrefix = prefix}),
            fixture.Service<ITenantContextAccessor>());

        normalizer.Normalize("abc").ShouldBe("app:abc");
    }

    [Fact]
    public void ShouldNormalizeKeysWithoutActiveTenant()
    {
        Normalizer.Normalize("abc").ShouldBe("abc");
        Normalizer.Normalize("abc", isTenantOwned: true).ShouldBe("abc");
    }

    [Fact]
    public void ShouldNormalizeKeysWithActiveTenant()
    {
        using var tenant = TenantContextAccessor.Change(Tenant);

        Normalizer.Normalize("abc").ShouldBe("abc");
        Normalizer.Normalize("abc", isTenantOwned: true)
            .ShouldBe($"tenant:{Tenant.Id:D}:abc");
    }

    // Contextualized key normalization

    [Fact]
    public void ShouldUseDerivedContextName()
    {
        Normalizer.Normalize<SomeCacheItem>("abc").ShouldBe("allegory:axiom:caching:some:abc");
    }

    [Fact]
    public void ShouldUseCacheNameAttribute()
    {
        Normalizer.Normalize<NamedCacheItem>("abc").ShouldBe("custom:name:abc");
    }

    [Fact]
    public void ShouldUseHostKeyWhenNoTenantIsActive()
    {
        Normalizer.Normalize<TenantOwnedCacheItem>("abc").ShouldBe("allegory:axiom:caching:tenant-owned:abc");
    }

    [Fact]
    public void ShouldIncludeTenantIdForTenantOwnedCache()
    {
        using var tenant = TenantContextAccessor.Change(Tenant);

        Normalizer
            .Normalize<TenantOwnedCacheItem>("abc")
            .ShouldBe($"tenant:{Tenant.Id:D}:allegory:axiom:caching:tenant-owned:abc");
    }

    [Fact]
    public void ShouldUseHostKeyForNonTenantOwnedCache()
    {
        using var tenant = TenantContextAccessor.Change(Tenant);

        Normalizer.Normalize<SomeCacheItem>("abc").ShouldBe("allegory:axiom:caching:some:abc");
    }

    [Fact]
    public void ShouldPlaceKeyPrefixBeforeTenantAndContext()
    {
        var normalizer = new CacheKeyNormalizer(
            Options.Create(new CacheOptions {KeyPrefix = "app:"}),
            TenantContextAccessor);

        using var tenant = TenantContextAccessor.Change(Tenant);

        normalizer.Normalize<TenantOwnedCacheItem>("abc")
            .ShouldBe($"app:tenant:{Tenant.Id:D}:allegory:axiom:caching:tenant-owned:abc");
    }
}