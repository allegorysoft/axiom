using System.Security.Principal;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Exceptions;
using Allegory.Axiom.Security.Principal;

namespace Allegory.Axiom.MultiTenancy;

public class CurrentTenantChecker(
    ITenantPrincipalStore tenantPrincipalStore,
    IPrincipalAccessor principalAccessor)
    : ICurrentTenantChecker, ISingletonService
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

        var principalId = identity.GetNameIdentifier();

        if (!await TenantPrincipalStore.HasAccessAsync(principalId, tenant.Id))
        {
            throw new AuthorizationException(MultiTenancyExceptionCodes.PrincipalHasNoAccess)
                .AddData("principalId", principalId)
                .AddData("tenantId", tenant.Id);
        }
    }
}