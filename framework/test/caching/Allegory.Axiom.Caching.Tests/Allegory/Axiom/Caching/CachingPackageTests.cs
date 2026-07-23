using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Caching;

public class CachingPackageTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task ShouldApplyHybridDelegateToHybridCacheOptions()
    {
        var expiration = TimeSpan.FromMinutes(11);

        var services = await fixture.CreateServiceProviderAsync(builder =>
        {
            builder.Services.Configure<CacheOptions>(o =>
                o.ConfigureHybrid = h => h.DefaultEntryOptions = new HybridCacheEntryOptions {Expiration = expiration});
        });

        services.GetRequiredService<IOptions<HybridCacheOptions>>()
            .Value.DefaultEntryOptions!.Expiration.ShouldBe(expiration);
    }

    [Fact]
    public async Task ShouldAppendSeparatorToKeyPrefix()
    {
        var services = await fixture.CreateServiceProviderAsync(builder =>
        {
            builder.Services.Configure<CacheOptions>(o => o.KeyPrefix = "  app  ");
        });

        services.GetRequiredService<IOptions<CacheOptions>>().Value.KeyPrefix.ShouldBe("app:");
    }

    [Fact]
    public async Task ShouldLeaveKeyPrefixWithTrailingSeparatorUnchanged()
    {
        var services = await fixture.CreateServiceProviderAsync(builder =>
        {
            builder.Services.Configure<CacheOptions>(o => o.KeyPrefix = "app:");
        });

        services.GetRequiredService<IOptions<CacheOptions>>().Value.KeyPrefix.ShouldBe("app:");
    }
}