using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Caching;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Setting;

public class SettingValueStore(
    IUnitOfWorkManager unitOfWorkManager,
    ISettingValueRecordRepository repository,
    ICacheKeyNormalizer keyNormalizer,
    HybridCache hybridCache) :
    ISettingValueStore, ISingletonService
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected ISettingValueRecordRepository Repository { get; } = repository;
    public ICacheKeyNormalizer KeyNormalizer { get; } = keyNormalizer;
    protected HybridCache HybridCache { get; } = hybridCache;

    public virtual ValueTask<SettingValue?> FindAsync(
        SettingDefinition definition,
        string contributorName,
        string? contributorKey)
    {
        return HybridCache.GetOrCreateAsync(
            $"settings:values:{definition.Name}:{contributorName}:{contributorKey ?? "<null>"}",
            (instance: this, definition, contributorName, contributorKey),
            static async (state, ct) =>
            {
                await using var uow = state.instance.UnitOfWorkManager.Begin(
                    UnitOfWorkOptions.Suppress, cancellationToken: ct);

                var result = await state.instance.Repository.FindAsync(
                    e => e.Name == state.definition.Name &&
                         e.ContributorName == state.contributorName &&
                         e.ContributorKey == state.contributorKey,
                    cancellationToken: CancellationToken.None);

                return result is null ? null : new SettingValue(JsonElement.Parse(result.Value));
            });
    }

    public virtual Task SetAsync<T>(
        SettingDefinition definition,
        string contributorName,
        string? contributorKey,
        T value,
        bool checkFallbackValue = true)
    {
        throw new System.NotImplementedException();
    }
}