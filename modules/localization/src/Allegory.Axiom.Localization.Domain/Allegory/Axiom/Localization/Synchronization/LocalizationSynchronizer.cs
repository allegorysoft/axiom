using System.Collections.Frozen;
using System.IO.Hashing;
using System.Text;
using System.Threading.Tasks;
using Allegory.Axiom.Caching;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.DistributedLocking;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.Localization;

public class LocalizationSynchronizer(
    ILogger<LocalizationSynchronizer> logger,
    ILocalizationProvider localizationProvider,
    IDistributedLock distributedLock,
    LocalizationSerializer serializer,
    ICache cache,
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
    protected ICache Cache { get; } = cache;
    protected ICacheKeyNormalizer CacheKeyNormalizer { get; } = cacheKeyNormalizer;
    protected HybridCache HybridCache { get; } = hybridCache;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected ILocalizationRecordRepository Repository { get; } = repository;

    public virtual async Task PushAsync()
    {
        foreach (var context in LocalizationProvider.Contexts)
        {
            await PushAsync(context.Key, context.Value);
        }
    }

    protected virtual async Task PushAsync(string name, LocalizationContext context)
    {
        await using var handle = await DistributedLock.TryAcquireAsync(name);

        if (handle == null)
        {
            Logger.LogSynchronizationSkipped(name);
            return;
        }

        foreach (var (culture, translations) in context.Translations)
        {
            await PushAsync(name, context, culture, translations);
        }
    }

    protected virtual async Task PushAsync(
        string name,
        LocalizationContext context,
        string culture,
        FrozenDictionary<string, string> translations)
    {
        //localizations:sync:name:culture
        var key = CacheKeyNormalizer.Normalize($"localizations:sync:{name}:{culture}");
        var cachedHash = await HybridCache.GetOrCreateAsync(
            key,
            static _ => ValueTask.FromResult((ulong) 0));

        var json = Serializer.Serialize(translations);
        var hash = XxHash64.HashToUInt64(Encoding.UTF8.GetBytes(json));

        if (hash == cachedHash)
        {
            Logger.LogSynchronizationSkippedNoChanges(name, culture);
            return;
        }

        await using var uow = UnitOfWorkManager.Begin();
        var record = await Repository.FindAsync(e => e.Name == name && e.Culture == culture);
    }

    //Pull
}