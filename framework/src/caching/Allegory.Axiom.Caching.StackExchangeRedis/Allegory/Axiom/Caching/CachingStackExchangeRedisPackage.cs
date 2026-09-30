using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MicrosoftRedisCacheOptions = Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions;

namespace Allegory.Axiom.Caching;

internal sealed class CachingStackExchangeRedisPackage : IConfigureApplication
{
    internal const string RedisOptionsKey = CacheOptions.Section + ":Redis";

    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<CacheOptions>(options =>
        {
            builder.Configuration.GetSection(RedisOptionsKey).Bind(options.Redis);
        });

        builder.Services.AddStackExchangeRedisCache(_ => { });

        builder.Services
            .AddOptions<MicrosoftRedisCacheOptions>()
            .Configure<RedisConnectionFactory, IOptions<CacheOptions>>((options, factory, cacheOptions) =>
            {
                var redisCacheOptions = cacheOptions.Value.Redis;
                options.ConnectionMultiplexerFactory = () => factory.GetAsync(redisCacheOptions.ConnectionName).AsTask();

                // Both the Microsoft (MicrosoftRedisCacheOptions) and Axiom (CacheOptions.Redis) cache
                // option types are bound from the same "Axiom:Cache:Redis" configuration section in
                // appsettings.json, so a single section drives both providers.
                //
                // Responsibilities split as follows:
                //   - ConnectionName  : Axiom-specific; resolves the connection via RedisConnectionFactory.
                //   - InstanceName    : Microsoft-specific; used as the key prefix for cache entries.
                //
                // Binding order matters: we bind the raw configuration first, then let the Axiom-side
                // `Configure` callback apply any programmatic overrides on top of it (last-write-wins).
                builder.Configuration.GetSection(RedisOptionsKey).Bind(options);
                redisCacheOptions.Configure?.Invoke(options);
            });

        return Task.CompletedTask;
    }
}