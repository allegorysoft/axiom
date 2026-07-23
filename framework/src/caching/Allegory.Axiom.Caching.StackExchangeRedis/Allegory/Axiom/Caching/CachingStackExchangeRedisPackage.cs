using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Caching;

internal sealed class CachingStackExchangeRedisPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            builder.Configuration.GetSection("Axiom:Cache:Redis").Bind(options);
        });

        builder.Services
            .AddOptions<RedisCacheOptions>()
            .Configure<IOptions<CacheOptions>>((options, cacheOptions) =>
            {
                cacheOptions.Value.ConfigureRedis?.Invoke(options);
            });

        return Task.CompletedTask;
    }
}