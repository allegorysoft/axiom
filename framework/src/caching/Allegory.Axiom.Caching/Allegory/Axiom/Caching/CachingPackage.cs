using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Caching;

internal sealed class CachingPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        var cacheBuilder = builder.Services.AddHybridCache();
        builder.AddBuilder(cacheBuilder);

        builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection("Axiom:Cache"));
        builder.Services.PostConfigure<CacheOptions>(o =>
        {
            if (string.IsNullOrEmpty(o.KeyPrefix)) return;

            var prefix = o.KeyPrefix.Trim();
            o.KeyPrefix = prefix.EndsWith(':') ? prefix : prefix + ':';
        });

        builder.Services
            .AddOptions<HybridCacheOptions>()
            .Configure<IOptions<CacheOptions>>((options, cacheOptions) =>
            {
                cacheOptions.Value.ConfigureHybrid?.Invoke(options);
            });

        return Task.CompletedTask;
    }
}