using System.Threading.Tasks;
using Allegory.Axiom.Domain.Entities.Events;
using Allegory.Axiom.EventBus.Local;
using Allegory.Axiom.Priority;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Setting;

public class SettingValueCacheInvalidator(
    SettingValueCacheKeyNormalizer keyNormalizer,
    HybridCache cache,
    IUnitOfWorkManager unitOfWorkManager) :
    ILocalEventHandler<EntityChanged<SettingValueRecord>>
{
    protected HybridCache Cache { get; } = cache;
    protected SettingValueCacheKeyNormalizer KeyNormalizer { get; } = keyNormalizer;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    public Task HandleAsync(EntityChanged<SettingValueRecord> payload)
    {
        var uow = UnitOfWorkManager.RequiredCurrent;
        var entity = payload.Entity;

        var key = KeyNormalizer.Normalize(entity.Name, entity.Identifier);

        uow.AddHook(
            UnitOfWorkHookPoint.AfterComplete,
            async () => { await Cache.RemoveAsync(key); },
            PriorityLevel.Highest);

        return Task.CompletedTask;
    }
}