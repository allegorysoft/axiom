using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Caching;

public readonly record struct CacheTypeDescriptor
{
    public string Name { get; init; }
    public bool IsTenantAgnostic { get; init; }
    public HybridCacheEntryOptions? EntryOptions { get; init; }
}