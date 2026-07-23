using System;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Caching;

public static class CacheOptionsExtensions
{
    internal const string ConfigureHybridKey = "Hybrid";

    extension(CacheOptions options)
    {
        public Action<HybridCacheOptions>? ConfigureHybrid
        {
            get => options.ExtraProperties.TryGetValue(ConfigureHybridKey, out var value)
                ? (Action<HybridCacheOptions>?) value
                : null;

            set => options.ExtraProperties[ConfigureHybridKey] = value;
        }
    }
}