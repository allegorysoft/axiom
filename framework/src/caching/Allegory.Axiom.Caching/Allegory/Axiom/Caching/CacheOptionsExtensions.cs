using System;
using Allegory.Axiom.Extensibility;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Caching;

public static class CacheOptionsExtensions
{
    internal const string ConfigureHybridKey = "Hybrid";

    extension(CacheOptions options)
    {
        public Action<HybridCacheOptions>? ConfigureHybrid
        {
            get => options.TryGetProperty<Action<HybridCacheOptions>>(ConfigureHybridKey);
            set => options.SetProperty(ConfigureHybridKey, value);
        }
    }
}