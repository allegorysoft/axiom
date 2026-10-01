using Allegory.Axiom.Extensibility;

namespace Allegory.Axiom.Caching;

public static class CacheOptionsExtensions
{
    extension(CacheOptions options)
    {
        public RedisCacheOptions Redis
        {
            get => options.GetOrAddProperty(
                CachingRedisPackage.Section,
                static () => new RedisCacheOptions());

            set => options.SetProperty(CachingRedisPackage.Section, value);
        }
    }
}