using System;
using System.Collections.Frozen;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Caching;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.DistributedLocking;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.Localization;

public class LocalizationSynchronizer(
    ILogger<LocalizationSynchronizer> logger,
    ILocalizationProvider localizationProvider,
    IDistributedLock distributedLock,
    LocalizationSerializer serializer,
    ICacheKeyNormalizer cacheKeyNormalizer,
    HybridCache hybridCache,
    IUnitOfWorkManager unitOfWorkManager,
    ILocalizationRecordRepository repository) :
    ISingletonService
{
    protected ILogger<LocalizationSynchronizer> Logger { get; } = logger;
    protected ILocalizationProvider LocalizationProvider { get; } = localizationProvider;
    protected IDistributedLock DistributedLock { get; } = distributedLock;
    protected LocalizationSerializer Serializer { get; } = serializer;
    protected ICacheKeyNormalizer CacheKeyNormalizer { get; } = cacheKeyNormalizer;
    protected HybridCache HybridCache { get; } = hybridCache;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected ILocalizationRecordRepository Repository { get; } = repository;
    protected HybridCacheEntryOptions ReadCacheOptions { get; } = new() {Expiration = TimeSpan.FromDays(1)};

    protected HybridCacheEntryOptions HashReadCacheOptions { get; } = new()
    {
        Flags = HybridCacheEntryFlags.DisableUnderlyingData
    };

    protected HybridCacheEntryOptions HashWriteCacheOptions { get; } = new()
    {
        Expiration = TimeSpan.FromDays(30),
        Flags = HybridCacheEntryFlags.DisableLocalCache
    };

    public virtual async Task SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (name, context) in LocalizationProvider.Contexts)
        {
            await SynchronizeAsync(name, context, cancellationToken);
        }
    }

    protected virtual async Task SynchronizeAsync(
        string name,
        LocalizationContext context,
        CancellationToken cancellationToken = default)
    {
        await using var handle = await DistributedLock.TryAcquireAsync(
            $"localizations:syncing:{name}",
            cancellationToken: cancellationToken);

        if (handle is null)
        {
            Logger.LogSynchronizationSkipped(name);
            return;
        }

        foreach (var (culture, translations) in context.Translations)
        {
            await SynchronizeAsync(name, context, culture, translations, cancellationToken);
        }
    }

    protected virtual async Task SynchronizeAsync(
        string name,
        LocalizationContext context,
        string culture,
        FrozenDictionary<string, string> translations,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyNormalizer.Normalize($"localizations:sync:{name}:{culture}");
        var cachedHash = await HybridCache.GetOrCreateAsync(
            cacheKey,
            static _ => ValueTask.FromResult<ulong>(0), // Never called because DisableUnderlyingData flag
            HashReadCacheOptions,
            cancellationToken: cancellationToken);

        var json = Serializer.Serialize(translations);
        var hash = XxHash64.HashToUInt64(Encoding.UTF8.GetBytes(json));

        if (hash == cachedHash)
        {
            Logger.LogSynchronizationSkippedNoChanges(name, culture);
            return;
        }

        await using var uow = UnitOfWorkManager.Begin(cancellationToken: cancellationToken);

        var record = await Repository.FindAsync(
            x => x.Name == name && x.Culture == culture,
            cancellationToken: CancellationToken.None);
        var translationDictionary = translations.ToDictionary();

        if (record is null)
        {
            await Repository.AddAsync(
                new LocalizationRecord(name, culture, context.DefaultCulture == culture)
                {
                    Translations = translationDictionary
                }, cancellationToken: CancellationToken.None);
        }
        else
        {
            record.IsDefault = context.DefaultCulture == culture;
            record.Translations = translationDictionary;
        }

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, async () =>
        {
            await HybridCache.SetAsync(
                cacheKey,
                hash,
                HashWriteCacheOptions,
                cancellationToken: CancellationToken.None);
            Logger.LogSynchronized(name, culture);
        });

        await uow.TryCompleteAsync(CancellationToken.None);
    }

    public virtual async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        var cache = await GetResourceListAsync(cancellationToken);

        foreach (var resource in cache.Resources)
        {
            var context = LocalizationProvider.Contexts.GetOrAdd(
                resource.Name,
                static (_, r) => new LocalizationContext
                {
                    DefaultCulture = r.DefaultCulture
                },
                resource);

            foreach (var culture in resource.Cultures)
            {
                var translationCache = await GetTranslationAsync(resource, culture, cancellationToken);
                context.Translations[culture] = translationCache.Translations;
            }
        }
    }

    protected virtual ValueTask<LocalizationCacheItem> GetResourceListAsync(
        CancellationToken cancellationToken = default)
    {
        var key = CacheKeyNormalizer.Normalize("localizations:resources");

        return HybridCache.GetOrCreateAsync(
            key,
            this,
            static async (instance, ct) =>
            {
                await using var uow = instance.UnitOfWorkManager.Begin(
                    cancellationToken: ct,
                    options: UnitOfWorkOptions.Suppress);

                var resources = await instance.Repository.GetResourceListAsync(
                    cancellationToken: CancellationToken.None);

                return new LocalizationCacheItem
                {
                    Resources = resources
                };
            },
            ReadCacheOptions,
            cancellationToken: cancellationToken);
    }

    protected virtual ValueTask<LocalizationTranslationCacheItem> GetTranslationAsync(
        LocalizationResourceProjection resource,
        string culture,
        CancellationToken cancellationToken = default)
    {
        var key = CacheKeyNormalizer.Normalize($"localizations:translations:{resource.Name}:{culture}");

        return HybridCache.GetOrCreateAsync(
            key,
            (instance: this, resource, culture),
            static async (state, ct) =>
            {
                await using var uow = state.instance.UnitOfWorkManager.Begin(
                    cancellationToken: ct,
                    options: UnitOfWorkOptions.Suppress);

                var record = await state.instance.Repository.FindAsync(
                    e => e.Culture == state.culture && e.Name == state.resource.Name,
                    cancellationToken: CancellationToken.None);

                if (record is null)
                {
                    state.instance.Logger.LogTranslationNotFound(state.resource.Name, state.culture);

                    return new LocalizationTranslationCacheItem
                    {
                        Translations = FrozenDictionary<string, string>.Empty
                    };
                }

                state.instance.Logger.LogTranslationsLoadedFromDatabase(state.resource.Name, state.culture);

                return new LocalizationTranslationCacheItem
                {
                    Translations = record.Translations.ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value)
                };
            },
            ReadCacheOptions,
            cancellationToken: cancellationToken);
    }
}