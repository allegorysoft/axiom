using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.MultiTenancy;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Caching;

public class Cache(
    HybridCache hybridCache,
    IOptions<CacheOptions> options,
    ITenantContextAccessor tenantAccessor)
    : ICache, ISingletonService
{
    // How should interact with unit of work ?
    protected HybridCache HybridCache { get; } = hybridCache;
    protected CacheOptions Options { get; } = options.Value;
    protected ITenantContextAccessor TenantAccessor { get; } = tenantAccessor;
    protected ConcurrentDictionary<Type, CacheTypeDescriptor> CacheTypeDescriptors { get; } = new();

    public virtual ValueTask<T> GetOrCreateAsync<TState, T>(
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var descriptor = CacheTypeDescriptors.GetOrAdd(typeof(T), GetCacheTypeDescriptor, Options);

        return HybridCache.GetOrCreateAsync(
            NormalizeKey(key, descriptor),
            state,
            factory,
            options: options ?? descriptor.EntryOptions,
            tags: tags,
            cancellationToken: cancellationToken);
    }

    public virtual ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var descriptor = CacheTypeDescriptors.GetOrAdd(typeof(T), GetCacheTypeDescriptor, Options);

        return HybridCache.GetOrCreateAsync(
            NormalizeKey(key, descriptor),
            factory,
            options: options ?? descriptor.EntryOptions,
            tags: tags,
            cancellationToken: cancellationToken);
    }

    public virtual ValueTask SetAsync<T>(
        string key,
        T value,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var descriptor = CacheTypeDescriptors.GetOrAdd(typeof(T), GetCacheTypeDescriptor, Options);

        return HybridCache.SetAsync(
            NormalizeKey(key, descriptor),
            value,
            options: options ?? descriptor.EntryOptions,
            tags: tags,
            cancellationToken: cancellationToken);
    }

    public virtual ValueTask RemoveAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var descriptor = CacheTypeDescriptors.GetOrAdd(typeof(T), GetCacheTypeDescriptor, Options);

        return HybridCache.RemoveAsync(NormalizeKey(key, descriptor), cancellationToken: cancellationToken);
    }

    public virtual ValueTask RemoveAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var descriptor = CacheTypeDescriptors.GetOrAdd(typeof(T), GetCacheTypeDescriptor, Options);

        return HybridCache.RemoveAsync(
            keys.Select(key => NormalizeKey(key, descriptor)),
            cancellationToken: cancellationToken);
    }

    public virtual ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        return HybridCache.RemoveByTagAsync(tag, cancellationToken: cancellationToken);
    }

    public virtual ValueTask RemoveByTagAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        return HybridCache.RemoveByTagAsync(tags, cancellationToken: cancellationToken);
    }

    protected virtual string NormalizeKey(string key, CacheTypeDescriptor descriptor)
    {
        if (descriptor.IsTenantAgnostic || TenantAccessor.Current is null)
        {
            //Host: prefix:{context}:{key}
            return $"{Options.KeyPrefix}{descriptor.Name}:{key}";
        }

        //Tenant: prefix:tenant:{tenant-id}:{context}:{key}
        return $"{Options.KeyPrefix}tenant:{TenantAccessor.Current.Id:D}:{descriptor.Name}:{key}";
    }

    protected static CacheTypeDescriptor GetCacheTypeDescriptor(Type type, CacheOptions options)
    {
        options.Types.TryGetValue(type, out var item);

        return new CacheTypeDescriptor
        {
            Name = item?.Name ?? GetContextName(type),
            IsTenantAgnostic = type.IsDefined(typeof(TenantAgnosticAttribute), false),
            EntryOptions = item?.EntryOptions,
        };

        static string GetContextName(Type t) =>
            JsonNamingPolicy.KebabCaseLower.ConvertName(
                Strip(CacheNameAttribute.Get(t).Replace('.', ':')));

        static string Strip(string name) =>
            name.EndsWith("CacheItem", StringComparison.Ordinal)
                ? name[..^9]
                : name;
    }
}