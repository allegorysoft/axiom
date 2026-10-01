using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace Allegory.Axiom.Redis;

public class RedisOptions : Dictionary<string, RedisOption>
{
    public const string DefaultConnectionName = "Default";
}

public class RedisOption
{
    public string? Configuration { get; set; }
    public ConfigurationOptions? ConfigurationOptions { get; set; }
    public Action<ConfigurationOptions>? ConfigureOptionsAction { get; set; }
}