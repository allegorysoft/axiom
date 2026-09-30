using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;
using MicrosoftRedisCacheOptions = Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions;

namespace Allegory.Axiom.Caching;

internal sealed class CachingFusionCachePackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        var cacheBuilder = builder.Services
            .AddFusionCache()
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .WithDistributedCache(sp => sp.GetRequiredService<IDistributedCache>())
            .WithBackplane(sp =>
            {
                var options = sp.GetRequiredService<IOptions<MicrosoftRedisCacheOptions>>().Value;

                return new RedisBackplane(
                    new RedisBackplaneOptions
                    {
                        Configuration = options.Configuration,
                        ConfigurationOptions = options.ConfigurationOptions,
                        ConnectionMultiplexerFactory = options.ConnectionMultiplexerFactory
                    },
                    sp.GetRequiredService<ILogger<RedisBackplane>>());
            })
            .AsHybridCache();
        builder.AddBuilder(cacheBuilder);

        return Task.CompletedTask;
    }
}