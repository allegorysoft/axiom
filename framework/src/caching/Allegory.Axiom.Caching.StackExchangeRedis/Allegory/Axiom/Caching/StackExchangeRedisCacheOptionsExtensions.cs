using Allegory.Axiom.Extensibility;

namespace Allegory.Axiom.Caching;

public static class StackExchangeRedisCacheOptionsExtensions
{
    extension(CacheOptions options)
    {
        public RedisCacheOptions Redis
        {
            get => options.GetOrAddProperty(
                CachingStackExchangeRedisPackage.RedisOptionsKey,
                static () => new RedisCacheOptions());

            set => options.SetProperty(CachingStackExchangeRedisPackage.RedisOptionsKey, value);
        }
    }
}