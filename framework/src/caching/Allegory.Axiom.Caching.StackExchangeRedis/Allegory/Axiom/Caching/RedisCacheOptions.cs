using System;
using Allegory.Axiom.Redis;

namespace Allegory.Axiom.Caching;

public class RedisCacheOptions
{
    public string ConnectionName { get; set; } = RedisOptions.DefaultConnectionName;
    public Action<Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions>? Configure { get; set; }
}