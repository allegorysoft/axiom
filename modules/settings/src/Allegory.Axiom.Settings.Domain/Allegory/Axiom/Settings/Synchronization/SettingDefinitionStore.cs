using System;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Caching;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Settings.Synchronization;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Settings;

public class SettingDefinitionStore(
    ICacheKeyNormalizer cacheKeyNormalizer,
    HybridCache hybridCache,
    ISettingDefinitionRecordRepository repository,
    IUnitOfWorkManager unitOfWorkManager) :
    ISettingDefinitionStore, ISingletonService
{
    protected HybridCache HybridCache { get; } = hybridCache;
    protected ISettingDefinitionRecordRepository Repository { get; } = repository;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    protected string CacheKey { get; } = cacheKeyNormalizer.Normalize("settings:definitions");
    protected HybridCacheEntryOptions CacheOptions { get; } = new() {Expiration = TimeSpan.FromDays(1)};

    public virtual async ValueTask<ImmutableArray<SettingDefinition>> GetListAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.Values;
    }

    public virtual async ValueTask<SettingDefinition?> FindAsync(string name)
    {
        var settings = await GetSettingsAsync();
        settings.TryGetValue(name, out var setting);
        return setting;
    }

    protected virtual async ValueTask<FrozenDictionary<string, SettingDefinition>> GetSettingsAsync()
    {
        var cache = await HybridCache.GetOrCreateAsync(
            CacheKey,
            this,
            static async (instance, ct) =>
            {
                await using var uow = instance.UnitOfWorkManager.Begin(
                    cancellationToken: ct,
                    options: UnitOfWorkOptions.Suppress);

                var settings = await instance.Repository.GetDefinitionListAsync(
                    cancellationToken: CancellationToken.None);

                return new SettingDefinitionCacheItem
                {
                    Settings = settings.ToFrozenDictionary(kvp => kvp.Name, kvp => kvp)
                };
            },
            CacheOptions);

        return cache.Settings;
    }
}