using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Priority;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Caching;

public class Cache(
    HybridCache hybridCache,
    ICacheKeyNormalizer keyNormalizer,
    IUnitOfWorkManager unitOfWorkManager) :
    ICache, ISingletonService
{
    protected HybridCache HybridCache { get; } = hybridCache;
    protected ICacheKeyNormalizer KeyNormalizer { get; } = keyNormalizer;
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
        var descriptor = KeyNormalizer.GetDescriptor<T>();

        return HybridCache.GetOrCreateAsync(
            KeyNormalizer.Normalize(key, descriptor),
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
        var descriptor = KeyNormalizer.GetDescriptor<T>();

        return HybridCache.GetOrCreateAsync(
            KeyNormalizer.Normalize(key, descriptor),
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
        var descriptor = KeyNormalizer.GetDescriptor<T>();
        var normalizedKey = KeyNormalizer.Normalize(key, descriptor);
        var entryOptions = options ?? descriptor.EntryOptions;

        if (mutationMode is CacheMutationMode.Immediate || UnitOfWorkManager.Current is not { } unitOfWork)
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
        var normalizedKey = KeyNormalizer.Normalize<T>(key);

        if (mutationMode is CacheMutationMode.Immediate || UnitOfWorkManager.Current is not { } unitOfWork)
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
        var descriptor = KeyNormalizer.GetDescriptor<T>();

        if (mutationMode is CacheMutationMode.Immediate || UnitOfWorkManager.Current is not { } unitOfWork)
        {
            return HybridCache.RemoveAsync(
                keys.Select(key => KeyNormalizer.Normalize(key, descriptor)),
                cancellationToken);
        }

        var normalizedKeys = keys.Select(key => KeyNormalizer.Normalize(key, descriptor)).ToArray();

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
        if (mutationMode is CacheMutationMode.Immediate || UnitOfWorkManager.Current is not { } unitOfWork)
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
        if (mutationMode is CacheMutationMode.Immediate || UnitOfWorkManager.Current is not { } unitOfWork)
        {
            return HybridCache.RemoveByTagAsync(tags, cancellationToken: cancellationToken);
        }

        unitOfWork.AddHook(
            UnitOfWorkHookPoint.AfterComplete,
            () => HybridCache.RemoveByTagAsync(tags, cancellationToken).AsTask(),
            PriorityLevel.Highest);

        return ValueTask.CompletedTask;
    }
}