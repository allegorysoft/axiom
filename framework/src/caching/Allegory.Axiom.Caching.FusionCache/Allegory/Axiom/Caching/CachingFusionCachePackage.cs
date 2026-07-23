using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace Allegory.Axiom.Caching;

public class CachingFusionCachePackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        var cacheBuilder = builder.Services
            .AddFusionCache()
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .WithDistributedCache(sp => sp.GetRequiredService<IDistributedCache>())
            .WithBackplane(sp =>
            {
                var redisOptions = sp.GetRequiredService<IOptions<RedisCacheOptions>>().Value;

                return new RedisBackplane(
                    new RedisBackplaneOptions
                    {
                        Configuration = redisOptions.Configuration,
                        ConfigurationOptions = redisOptions.ConfigurationOptions,
                        ConnectionMultiplexerFactory = redisOptions.ConnectionMultiplexerFactory
                    },
                    sp.GetRequiredService<ILogger<RedisBackplane>>());
            })
            .AsHybridCache();
        builder.AddBuilder(cacheBuilder);

        return Task.CompletedTask;
    }
}