using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Caching;

public class CacheOptions
{
    public string? KeyPrefix { get; set; }
    public Dictionary<Type, CacheTypeOptions> Types { get; set; } = [];
}

public class CacheTypeOptions
{
    public string? Name { get; set; }
    public HybridCacheEntryOptions? EntryOptions { get; set; }
}