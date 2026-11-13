using System.Text.Json;
using System.Threading.Tasks;
using Allegory.Axiom.Caching.Converters;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
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

        AddCacheSerializerOption(builder.Services);
        ConfigureOptions(builder);

        return Task.CompletedTask;
    }

    private static void AddCacheSerializerOption(IServiceCollection collection)
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            Converters = {new FrozenDictionaryJsonConverter<string, string>()}
        };

        collection.AddKeyedSingleton<JsonSerializerOptions>(typeof(IHybridCacheSerializer<>), options);
    }

    private static void ConfigureOptions(IHostApplicationBuilder builder)
    {
        builder.Services
            .Configure<CacheOptions>(builder.Configuration.GetSection(CacheOptions.Section))
            .PostConfigure<CacheOptions>(o =>
            {
                if (string.IsNullOrEmpty(o.KeyPrefix)) return;

                var prefix = o.KeyPrefix.Trim();
                o.KeyPrefix = prefix.EndsWith(':') ? prefix : prefix + ':';
            });

        builder.Services
            .AddOptions<HybridCacheOptions>()
            .Configure<IOptions<CacheOptions>>((options, cacheOptions) =>
            {
                builder.Configuration
                    .GetSection(CacheOptions.Section + ":Hybrid")
                    .Bind(options);

                cacheOptions.Value.ConfigureHybrid?.Invoke(options);
            });
    }
}