using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Caching;

public interface ICache
{
    ValueTask<T> GetOrCreateAsync<TState, T>(
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    ValueTask SetAsync<T>(
        string key,
        T value,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CacheMutationMode mutationMode = CacheMutationMode.Immediate,
        CancellationToken cancellationToken = default);

    ValueTask RemoveAsync<T>(
        string key,
        CacheMutationMode mutationMode = CacheMutationMode.Immediate,
        CancellationToken cancellationToken = default);

    ValueTask RemoveAsync<T>(
        IEnumerable<string> keys,
        CacheMutationMode mutationMode = CacheMutationMode.Immediate,
        CancellationToken cancellationToken = default);

    ValueTask RemoveByTagAsync(
        string tag,
        CacheMutationMode mutationMode = CacheMutationMode.Immediate,
        CancellationToken cancellationToken = default);

    ValueTask RemoveByTagAsync(
        IEnumerable<string> tags,
        CacheMutationMode mutationMode = CacheMutationMode.Immediate,
        CancellationToken cancellationToken = default);
}