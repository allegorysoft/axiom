using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Caching;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Security.Encryption;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Setting;

public class SettingValueStore(
    IUnitOfWorkManager unitOfWorkManager,
    ISettingValueRecordRepository repository,
    ICacheKeyNormalizer keyNormalizer,
    HybridCache hybridCache,
    IStringEncryptionService stringEncryptionService) :
    ISettingValueStore, ISingletonService
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected ISettingValueRecordRepository Repository { get; } = repository;
    protected ICacheKeyNormalizer KeyNormalizer { get; } = keyNormalizer;
    protected HybridCache HybridCache { get; } = hybridCache;
    protected IStringEncryptionService StringEncryptionService { get; } = stringEncryptionService;

    public virtual ValueTask<SettingValue?> FindAsync(
        SettingDefinition definition,
        SettingValueIdentifier identifier)
    {
        return HybridCache.GetOrCreateAsync(
            $"settings:values:{definition.Name}:{identifier.Name}:{identifier.Key ?? "<null>"}",
            (instance: this, definition, identifier),
            static async (state, ct) =>
            {
                await using var uow = state.instance.UnitOfWorkManager.Begin(
                    UnitOfWorkOptions.Suppress, cancellationToken: ct);

                var result = await state.instance.Repository.FindAsync(
                    e => e.Name == state.definition.Name &&
                         e.Identifier == state.identifier,
                    cancellationToken: CancellationToken.None);

                return result is null ? null : new SettingValue(JsonElement.Parse(result.Value));
            });
    }

    public virtual async Task SetAsync<T>(
        SettingDefinition definition,
        SettingValueIdentifier identifier,
        T value,
        bool checkFallbackValue = true)
    {
        var v = JsonSerializer.Serialize(value);

        if (definition.IsEncrypted)
        {
            v = StringEncryptionService.Encrypt(v);
        }
    }
}