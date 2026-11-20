using System.Threading.Tasks;
using Allegory.Axiom.Caching;
using Allegory.Axiom.Domain.Entities.Events;
using Allegory.Axiom.EventBus.Local;
using Allegory.Axiom.Priority;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Settings;

public class SettingDefinitionCacheInvalidator(
    HybridCache cache,
    ICacheKeyNormalizer cacheKeyNormalizer,
    IUnitOfWorkManager unitOfWorkManager) :
    ILocalEventHandler<EntityChanged<SettingDefinitionRecord>>
{
    protected HybridCache Cache { get; } = cache;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected string CacheKey { get; } = cacheKeyNormalizer.Normalize("settings:definitions");

    public Task HandleAsync(EntityChanged<SettingDefinitionRecord> payload)
    {
        var uow = UnitOfWorkManager.RequiredCurrent;

        uow.AddHook(
            UnitOfWorkHookPoint.AfterComplete,
            async () => { await Cache.RemoveAsync(CacheKey); },
            PriorityLevel.Highest);

        return Task.CompletedTask;
    }
}