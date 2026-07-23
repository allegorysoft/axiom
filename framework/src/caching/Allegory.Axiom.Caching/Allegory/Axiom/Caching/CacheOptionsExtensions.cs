using System;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Caching;

public static class CacheOptionsExtensions
{
    extension(CacheOptions options)
    {
        public Action<HybridCacheOptions>? Hybrid
        {
            get => options.ExtraProperties.TryGetValue(
                CachingPackage.HybridOptionsKey, out var value)
                ? (Action<HybridCacheOptions>?) value
                : null;

            set => options.ExtraProperties[CachingPackage.HybridOptionsKey] = value;
        }
    }
}