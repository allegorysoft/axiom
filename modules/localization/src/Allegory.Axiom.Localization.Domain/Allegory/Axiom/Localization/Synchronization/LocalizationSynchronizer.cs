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

    public virtual async Task PushAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (name, context) in LocalizationProvider.Contexts)
        {
            await PushAsync(name, context, cancellationToken);
        }
    }

    protected virtual async Task PushAsync(
        string name,
        LocalizationContext context,
        CancellationToken cancellationToken = default)
    {
        await using var handle = await DistributedLock.TryAcquireAsync(
            $"localizations:sync:{name}",
            cancellationToken: cancellationToken);

        if (handle is null)
        {
            Logger.LogSynchronizationSkipped(name);
            return;
        }

        foreach (var (culture, translations) in context.Translations)
        {
            await PushAsync(name, context, culture, translations, cancellationToken);
        }
    }

    protected virtual async Task PushAsync(
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
            new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableUnderlyingData
            },
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
            record.Translations = translationDictionary;
        }

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, async () =>
        {
            await HybridCache.SetAsync(
                cacheKey,
                hash,
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromDays(30),
                    Flags = HybridCacheEntryFlags.DisableLocalCache
                }, cancellationToken: CancellationToken.None);
            Logger.LogSynchronized(name, culture);
        });

        await uow.TryCompleteAsync(CancellationToken.None);
    }

    public virtual async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var resources = await GetResourcesAsync(cancellationToken);

        foreach (var resource in resources.Items)
        {
            var context = LocalizationProvider.Contexts.GetOrAdd(
                resource.Name,
                static (_, r) => new LocalizationContext
                {
                    DefaultCulture = r.DefaultCulture
                }, resource);

            foreach (var culture in resource.Cultures)
            {
                var cacheItem = await GetTranslationsAsync(resource, culture, cancellationToken);

                if (cacheItem == null)
                {
                    Logger.LogTranslationNotFound(resource.Name, culture);
                }
                else
                {
                    context.Translations[culture] = cacheItem.Translations;
                }
            }
        }
    }

    protected virtual ValueTask<LocalizationResourcesCacheItem> GetResourcesAsync(
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

                var resources = await instance.Repository.GetResourceListAsync(ct);

                return new LocalizationResourcesCacheItem
                {
                    Items = resources
                };
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(1),
                Flags = HybridCacheEntryFlags.DisableDistributedCache
            },
            cancellationToken: cancellationToken);
    }

    protected virtual ValueTask<LocalizationTranslationCacheItem?> GetTranslationsAsync(
        LocalizationResourceProjection resource,
        string culture,
        CancellationToken cancellationToken = default)
    {
        // L2 caching is intentionally avoided to prevent redundant serialization,
        // deserialization, and memory allocations across distributed cache operations.
        // Falling back to the database when the L1 cache misses is sufficient.
        // Additionally, Translations uses FrozenDictionary, which requires a custom
        // System.Text.Json converter (e.g. FrozenDictionaryJsonConverter) for L2 serialization.

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
                    return null;
                }

                return new LocalizationTranslationCacheItem
                {
                    Translations = record.Translations.ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value)
                };
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(1),
                Flags = HybridCacheEntryFlags.DisableDistributedCache
            },
            cancellationToken: cancellationToken);
    }
}