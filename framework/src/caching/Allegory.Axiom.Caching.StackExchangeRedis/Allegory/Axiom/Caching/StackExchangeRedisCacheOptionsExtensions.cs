using System;
using Allegory.Axiom.Extensibility;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace Allegory.Axiom.Caching;

public static class StackExchangeRedisCacheOptionsExtensions
{
    internal const string ConfigureRedisKey = "Redis";

    extension(CacheOptions options)
    {
        public Action<RedisCacheOptions>? ConfigureRedis
        {
            get => options.TryGetProperty<Action<RedisCacheOptions>>(ConfigureRedisKey);
            set => options.SetProperty(ConfigureRedisKey, value);
        }
    }
}