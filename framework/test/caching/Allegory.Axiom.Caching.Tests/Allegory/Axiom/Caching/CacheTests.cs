using System;
using System.Threading.Tasks;
using Allegory.Axiom.MultiTenancy;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Allegory.Axiom.Caching;

public class CacheTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task Test1()
    {
        var cache = fixture.Service<ICache>();

        var y = await cache.GetOrCreateAsync(
            "abc",
            _ => ValueTask.FromResult(new SomeCacheItem()),
            cancellationToken: TestContext.Current.CancellationToken);
    }

    private void Comments()
    {
        // IMemoryCache, IDistributedCache, HybridCache
        // GetOrCreate; [string, ReadOnlySpan, DefaultInterpolatedStringHandler], [null, TState]
        // Set, Remove, RemoveByTag

        // Microsoft.Extensions.Caching.Abstractions -> Microsoft.Extensions.Caching.Memory -> Microsoft.Extensions.Caching.Hybrid
        // Microsoft.Extensions.Caching.StackExchangeRedis -> Imp of IDistributedCache for redis

        // ZiggyCreatures.FusionCache => Create additional package (use backplane)
        // ZiggyCreatures.FusionCache.Serialization.SystemTextJson
        // ZiggyCreatures.FusionCache.Backplane.StackExchangeRedis
    }
}

[TenantAgnostic]
public class SomeCacheItem {}