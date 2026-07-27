using System;
using System.Collections.Generic;
using Allegory.Axiom.Extensibility;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Caching;

public class CacheOptions : IExtraProperties
{
    public string? KeyPrefix { get; set; }
    public Dictionary<Type, CacheTypeOptions> Types { get; set; } = [];
    public IDictionary<string, object?> ExtraProperties { get; } = new Dictionary<string, object?>();
}

public class CacheTypeOptions
{
    public string? Name { get; set; }
    public HybridCacheEntryOptions? EntryOptions { get; set; }
}