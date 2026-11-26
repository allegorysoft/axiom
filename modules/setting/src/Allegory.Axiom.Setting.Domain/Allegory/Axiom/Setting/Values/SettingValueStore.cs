using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Setting;

public class SettingValueStore(
    IUnitOfWorkManager unitOfWorkManager,
    ISettingValueRecordRepository repository,
    SettingValueCacheKeyNormalizer keyNormalizer,
    HybridCache hybridCache,
    IOptions<SettingValueOptions> options) :
    ISettingValueStore, ISingletonService
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected ISettingValueRecordRepository Repository { get; } = repository;
    protected SettingValueCacheKeyNormalizer KeyNormalizer { get; } = keyNormalizer;
    protected HybridCache HybridCache { get; } = hybridCache;
    protected SettingValueOptions Options { get; } = options.Value;

    public virtual ValueTask<SettingValue?> FindAsync(string name, SettingValueIdentifier identifier)
    {
        return HybridCache.GetOrCreateAsync(
            KeyNormalizer.Normalize(name, identifier),
            (instance: this, name, identifier),
            static async (state, ct) =>
            {
                await using var uow = state.instance.UnitOfWorkManager.Begin(
                    UnitOfWorkOptions.Suppress, cancellationToken: ct);

                var result = await state.instance.Repository.FindAsync(
                    e => e.Name == state.name &&
                         e.Identifier == state.identifier,
                    cancellationToken: CancellationToken.None);

                return result is null ? null : new SettingValue(JsonElement.Parse(result.Value));
            });
    }

    public virtual async Task SetAsync(string name, SettingValueIdentifier identifier, SettingValue value)
    {
        await using var uow = UnitOfWorkManager.Begin();
        var entity = await Repository.FindAsync(e => e.Name == name && e.Identifier == identifier);

        if (entity is null)
        {
            entity = new SettingValueRecord(name, identifier, value.Value.GetRawText());
            await Repository.AddAsync(entity);
        }
        else
        {
            entity.SetValue(value.Value.GetRawText());
        }

        await uow.TryCompleteAsync();
    }

    public virtual async Task RemoveAsync(string name, SettingValueIdentifier identifier)
    {
        await using var uow = UnitOfWorkManager.Begin();
        var entity = await Repository.FindAsync(e => e.Name == name && e.Identifier == identifier);

        if (entity is not null)
        {
            await Repository.RemoveAsync(entity);
        }

        await uow.TryCompleteAsync();
    }
}