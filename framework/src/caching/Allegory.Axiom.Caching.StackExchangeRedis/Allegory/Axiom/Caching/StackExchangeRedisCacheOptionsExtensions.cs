using System;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace Allegory.Axiom.Caching;

public static class StackExchangeRedisCacheOptionsExtensions
{
    internal const string ConfigureRedisKey = "Redis";

    extension(CacheOptions options)
    {
        public Action<RedisCacheOptions>? ConfigureRedis
        {
            get => options.ExtraProperties.TryGetValue(ConfigureRedisKey, out var value)
                ? (Action<RedisCacheOptions>?) value
                : null;

            set => options.ExtraProperties[ConfigureRedisKey] = value;
        }
    }
}