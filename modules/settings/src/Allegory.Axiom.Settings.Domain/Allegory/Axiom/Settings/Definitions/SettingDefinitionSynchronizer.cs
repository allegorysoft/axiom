using System;
using System.IO.Hashing;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Caching;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.DistributedLocking;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.Settings;

public class SettingDefinitionSynchronizer(
    ILogger<SettingDefinitionSynchronizer> logger,
    ISettingDefinitionProvider settingDefinitionProvider,
    IDistributedLock distributedLock,
    ICacheKeyNormalizer cacheKeyNormalizer,
    HybridCache hybridCache,
    IUnitOfWorkManager unitOfWorkManager,
    ISettingDefinitionRecordRepository repository) :
    ISingletonService
{
    protected ILogger<SettingDefinitionSynchronizer> Logger { get; } = logger;
    protected ISettingDefinitionProvider SettingDefinitionProvider { get; } = settingDefinitionProvider;
    protected IDistributedLock DistributedLock { get; } = distributedLock;
    protected ICacheKeyNormalizer CacheKeyNormalizer { get; } = cacheKeyNormalizer;
    protected HybridCache HybridCache { get; } = hybridCache;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected ISettingDefinitionRecordRepository Repository { get; } = repository;

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
        foreach (var (_, setting) in SettingDefinitionProvider.Settings)
        {
            await SynchronizeAsync(setting, cancellationToken);
        }
    }

    protected virtual async Task SynchronizeAsync(
        SettingDefinition setting,
        CancellationToken cancellationToken = default)
    {
        await using var handle = await DistributedLock.TryAcquireAsync(
            $"settings:syncing:{setting.Name}",
            cancellationToken: cancellationToken);

        if (handle is null)
        {
            Logger.LogSynchronizationSkipped(setting.Name);
            return;
        }

        var cacheKey = CacheKeyNormalizer.Normalize($"settings:sync:{setting.Name}");
        var cachedHash = await HybridCache.GetOrCreateAsync(
            cacheKey,
            static _ => ValueTask.FromResult<ulong>(0), // Never called because DisableUnderlyingData flag
            HashReadCacheOptions,
            cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(setting);
        var hash = XxHash64.HashToUInt64(Encoding.UTF8.GetBytes(json));

        if (hash == cachedHash)
        {
            Logger.LogSynchronizationSkippedNoChanges(setting.Name);
            return;
        }

        await using var uow = UnitOfWorkManager.Begin(cancellationToken: cancellationToken);

        var record = await Repository.FindAsync(
            x => x.Name == setting.Name,
            cancellationToken: CancellationToken.None);

        if (record is null)
        {
            await Repository.AddAsync(
                new SettingDefinitionRecord(
                    setting.Name,
                    setting.DefaultValue,
                    setting.IsPublic,
                    setting.IsEncrypted,
                    setting.Contributors),
                cancellationToken: CancellationToken.None);
        }
        else
        {
            record.SetDefaultValue(setting.DefaultValue);
            record.IsPublic = setting.IsPublic;
            record.IsEncrypted = setting.IsEncrypted;
            record.Contributors = setting.Contributors;
        }

        uow.AddHook(UnitOfWorkHookPoint.AfterComplete, async () =>
        {
            await HybridCache.SetAsync(
                cacheKey,
                hash,
                HashWriteCacheOptions,
                cancellationToken: CancellationToken.None);
            Logger.LogSynchronized(setting.Name);
        });

        await uow.TryCompleteAsync(CancellationToken.None);
    }
}