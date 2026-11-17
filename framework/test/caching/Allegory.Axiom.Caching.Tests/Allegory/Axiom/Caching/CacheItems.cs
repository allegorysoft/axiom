using Allegory.Axiom.MultiTenancy;

namespace Allegory.Axiom.Caching;

public class SomeCacheItem;

[TenantOwned]
public class TenantOwnedCacheItem;

[CacheName("Custom.Name")]
public class NamedCacheItem;