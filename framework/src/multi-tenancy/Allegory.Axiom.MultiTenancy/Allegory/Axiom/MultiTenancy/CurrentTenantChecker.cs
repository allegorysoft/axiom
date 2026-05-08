using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Allegory.Axiom.Security.Principal;

namespace Allegory.Axiom.MultiTenancy;

public class CurrentTenantChecker(
    ITenantPrincipalStore tenantPrincipalStore,
    IPrincipalAccessor principalAccessor)
    : ICurrentTenantChecker
{
    protected ITenantPrincipalStore TenantPrincipalStore { get; } = tenantPrincipalStore;
    protected IPrincipalAccessor PrincipalAccessor { get; } = principalAccessor;

    public virtual async Task CheckAsync(TenantContext tenant)
    {
        var identity = PrincipalAccessor.Current?.Identity;

        if (identity is not {IsAuthenticated: true})
        {
            return;
        }

        var principalId = identity.FindId();

        if (string.IsNullOrWhiteSpace(principalId))
        {
            throw new Exception("Principal id not found");
        }

        if (!await TenantPrincipalStore.HasAccessAsync(principalId, tenant.Id))
        {
            throw new Exception($"Principal ({principalId}) does not have access to {tenant.Id}");
        }
    }
}