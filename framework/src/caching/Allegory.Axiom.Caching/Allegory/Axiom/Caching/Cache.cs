using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.Priority;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Caching;

public class Cache(
    HybridCache hybridCache,
    IOptions<CacheOptions> options,
    ITenantContextAccessor tenantContextAccessor,
    IUnitOfWorkManager unitOfWorkManager)
    : ICache, ISingletonService
{
    protected HybridCache HybridCache { get; } = hybridCache;
    protected CacheOptions Options { get; } = options.Value;
    protected ITenantContextAccessor TenantContextAccessor { get; } = tenantContextAccessor;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
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
        CacheMutationMode mutationMode = CacheMutationMode.Immediate,
        CancellationToken cancellationToken = default)
    {
        var descriptor = CacheTypeDescriptors.GetOrAdd(typeof(T), GetCacheTypeDescriptor, Options);
        var normalizedKey = NormalizeKey(key, descriptor);
        var entryOptions = options ?? descriptor.EntryOptions;

        if (mutationMode is CacheMutationMode.Immediate || UnitOfWorkManager.Current is not {} unitOfWork)
        {
            return HybridCache.SetAsync(normalizedKey, value, entryOptions, tags, cancellationToken);
        }

        unitOfWork.AddHook(
            UnitOfWorkHookPoint.AfterComplete,
            () => HybridCache.SetAsync(normalizedKey, value, entryOptions, tags, cancellationToken).AsTask(),
            PriorityLevel.Highest);

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask RemoveAsync<T>(
        string key,
        CacheMutationMode mutationMode = CacheMutationMode.Immediate,
        CancellationToken cancellationToken = default)
    {
        var descriptor = CacheTypeDescriptors.GetOrAdd(typeof(T), GetCacheTypeDescriptor, Options);
        var normalizedKey = NormalizeKey(key, descriptor);

        if (mutationMode is CacheMutationMode.Immediate || UnitOfWorkManager.Current is not {} unitOfWork)
        {
            return HybridCache.RemoveAsync(normalizedKey, cancellationToken);
        }

        unitOfWork.AddHook(
            UnitOfWorkHookPoint.AfterComplete,
            () => HybridCache.RemoveAsync(normalizedKey, cancellationToken).AsTask(),
            PriorityLevel.Highest);

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask RemoveAsync<T>(
        IEnumerable<string> keys,
        CacheMutationMode mutationMode = CacheMutationMode.Immediate,
        CancellationToken cancellationToken = default)
    {
        var descriptor = CacheTypeDescriptors.GetOrAdd(typeof(T), GetCacheTypeDescriptor, Options);
        if (mutationMode is CacheMutationMode.Immediate || UnitOfWorkManager.Current is not {} unitOfWork)
        {
            return HybridCache.RemoveAsync(
                keys.Select(key => NormalizeKey(key, descriptor)),
                cancellationToken);
        }

        var normalizedKeys = keys.Select(key => NormalizeKey(key, descriptor)).ToArray();

        unitOfWork.AddHook(
            UnitOfWorkHookPoint.AfterComplete,
            () => HybridCache.RemoveAsync(normalizedKeys, cancellationToken).AsTask(),
            PriorityLevel.Highest);

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask RemoveByTagAsync(
        string tag,
        CacheMutationMode mutationMode = CacheMutationMode.Immediate,
        CancellationToken cancellationToken = default)
    {
        if (mutationMode is CacheMutationMode.Immediate || UnitOfWorkManager.Current is not {} unitOfWork)
        {
            return HybridCache.RemoveByTagAsync(tag, cancellationToken);
        }

        unitOfWork.AddHook(
            UnitOfWorkHookPoint.AfterComplete,
            () => HybridCache.RemoveByTagAsync(tag, cancellationToken).AsTask(),
            PriorityLevel.Highest);

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask RemoveByTagAsync(
        IEnumerable<string> tags,
        CacheMutationMode mutationMode = CacheMutationMode.Immediate,
        CancellationToken cancellationToken = default)
    {
        if (mutationMode is CacheMutationMode.Immediate || UnitOfWorkManager.Current is not {} unitOfWork)
        {
            return HybridCache.RemoveByTagAsync(tags, cancellationToken: cancellationToken);
        }

        unitOfWork.AddHook(
            UnitOfWorkHookPoint.AfterComplete,
            () => HybridCache.RemoveByTagAsync(tags, cancellationToken).AsTask(),
            PriorityLevel.Highest);

        return ValueTask.CompletedTask;
    }

    protected virtual string NormalizeKey(string key, CacheTypeDescriptor descriptor)
    {
        if (descriptor.IsTenantAgnostic || TenantContextAccessor.Current is null)
        {
            //Host: prefix:{context}:{key}
            return $"{Options.KeyPrefix}{descriptor.Name}:{key}";
        }

        //Tenant: prefix:tenant:{tenant-id}:{context}:{key}
        return $"{Options.KeyPrefix}tenant:{TenantContextAccessor.Current.Id:D}:{descriptor.Name}:{key}";
    }

    protected static CacheTypeDescriptor GetCacheTypeDescriptor(Type type, CacheOptions options)
    {
        options.Types.TryGetValue(type, out var item);

        return new CacheTypeDescriptor
        {
            Name = item?.Name ?? GetContextName(type),
            IsTenantAgnostic = item?.IsTenantAgnostic ?? type.IsDefined(typeof(TenantAgnosticAttribute), false),
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