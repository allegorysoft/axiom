using Allegory.Axiom.Extensibility;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Setting;

public static class SettingOptionsExtensions
{
    internal const string SettingValueCacheOptions = "Cache";

    extension(SettingValueOptions options)
    {
        public HybridCacheEntryOptions? CacheOptions
        {
            get => options.TryGetProperty<HybridCacheEntryOptions?>(SettingValueCacheOptions);
            set => options.SetProperty(SettingValueCacheOptions, value);
        }
    }
}