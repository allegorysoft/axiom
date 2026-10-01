using System;
using Allegory.Axiom.Redis;
using MicrosoftRedisCacheOptions = Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions;

namespace Allegory.Axiom.Caching;

public class RedisCacheOptions
{
    public string ConnectionName { get; set; } = RedisOptions.DefaultConnectionName;
    public Action<MicrosoftRedisCacheOptions>? Configure { get; set; }
}