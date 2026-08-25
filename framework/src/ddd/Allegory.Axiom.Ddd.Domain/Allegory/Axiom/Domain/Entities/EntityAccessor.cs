using Allegory.Axiom.MultiTenancy;

namespace Allegory.Axiom.Domain.Entities;

public static class EntityAccessor
{
    public static void TrySetTenant(IEntity entity)
    {
        if (entity is not ITenantOwned)
        {
            return;
        }

        var tenant = ITenantContextAccessor.TryGetCurrent();
        if (tenant == null)
        {
            return;
        }

        ObjectAccessor.TrySetProperty(entity, nameof(ITenantOwned.TenantId), tenant.Id);
    }
}