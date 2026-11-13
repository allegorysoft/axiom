using System.Threading.Tasks;
using Allegory.Axiom.Caching;
using Allegory.Axiom.Domain.Entities.Events;
using Allegory.Axiom.EventBus.Local;
using Allegory.Axiom.Priority;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;

namespace Allegory.Axiom.Localization;

public class LocalizationInvalidator(
    HybridCache cache,
    ICacheKeyNormalizer cacheKeyNormalizer,
    IUnitOfWorkManager unitOfWorkManager) :
    ILocalEventHandler<EntityChanged<LocalizationRecord>>
{
    protected HybridCache Cache { get; } = cache;
    protected ICacheKeyNormalizer CacheKeyNormalizer { get; } = cacheKeyNormalizer;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    public Task HandleAsync(EntityChanged<LocalizationRecord> payload)
    {
        var uow = UnitOfWorkManager.RequiredCurrent;

        if (payload.ChangeType is EntityChangeType.Created or EntityChangeType.Deleted)
        {
            var cacheKey = CacheKeyNormalizer.Normalize("localizations:resources");

            uow.AddHook(
                UnitOfWorkHookPoint.AfterComplete,
                async () => { await Cache.RemoveAsync(cacheKey); },
                PriorityLevel.Highest);
        }

        var key = CacheKeyNormalizer.Normalize(
            $"localizations:translations:{payload.Entity.Name}:{payload.Entity.Culture}");

        uow.AddHook(
            UnitOfWorkHookPoint.AfterComplete,
            async () => { await Cache.RemoveAsync(key); },
            PriorityLevel.Highest);

        return Task.CompletedTask;
    }
}