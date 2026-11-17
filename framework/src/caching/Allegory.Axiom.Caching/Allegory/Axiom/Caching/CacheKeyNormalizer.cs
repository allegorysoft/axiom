using System;
using System.Collections.Concurrent;
using System.Text.Json;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.MultiTenancy;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Caching;

public class CacheKeyNormalizer(
    IOptions<CacheOptions> options,
    ITenantContextAccessor tenantContextAccessor) :
    ICacheKeyNormalizer, ISingletonService
{
    protected CacheOptions Options { get; } = options.Value;
    protected ITenantContextAccessor TenantContextAccessor { get; } = tenantContextAccessor;
    protected ConcurrentDictionary<Type, CacheTypeDescriptor> Descriptors { get; } = new();

    public virtual string Normalize(string key, bool isTenantOwned = false)
    {
        if (!isTenantOwned || TenantContextAccessor.Current is null)
        {
            //Host: prefix:{key}
            return $"{Options.KeyPrefix}{key}";
        }

        //Tenant: prefix:tenant:{tenant-id}:{key}
        return $"{Options.KeyPrefix}tenant:{TenantContextAccessor.Current.Id:D}:{key}";
    }

    public virtual string Normalize(string key, CacheTypeDescriptor descriptor)
    {
        if (!descriptor.IsTenantOwned || TenantContextAccessor.Current is null)
        {
            //Host: prefix:{context}:{key}
            return $"{Options.KeyPrefix}{descriptor.Name}:{key}";
        }

        //Tenant: prefix:tenant:{tenant-id}:{context}:{key}
        return $"{Options.KeyPrefix}tenant:{TenantContextAccessor.Current.Id:D}:{descriptor.Name}:{key}";
    }

    public virtual string Normalize<TCache>(string key)
    {
        var descriptor = GetDescriptor<TCache>();
        return Normalize(key, descriptor);
    }

    public virtual CacheTypeDescriptor GetDescriptor<TCache>()
    {
        return Descriptors.GetOrAdd(typeof(TCache), GetCacheTypeDescriptor, Options);
    }

    protected static CacheTypeDescriptor GetCacheTypeDescriptor(Type type, CacheOptions options)
    {
        options.Types.TryGetValue(type, out var item);

        return new CacheTypeDescriptor
        {
            Name = item?.Name ?? GetContextName(type),
            IsTenantOwned = item?.IsTenantOwned ?? type.IsDefined(typeof(TenantOwnedAttribute), true),
            EntryOptions = item?.EntryOptions,
        };

        static string GetContextName(Type t) =>
            JsonNamingPolicy.KebabCaseLower.ConvertName(
                Strip(CacheNameAttribute.Get(t).Replace('.', ':')));

        static string Strip(string name) =>
            name.EndsWith("CacheItem", StringComparison.OrdinalIgnoreCase)
                ? name[..^9]
                : name;
    }
}