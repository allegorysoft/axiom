using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Allegory.Axiom.MultiTenancy;

public class MultiTenancyMiddleware(
    RequestDelegate next,
    ICurrentTenantProvider currentTenantProvider,
    ITenantContextAccessor tenantContextAccessor)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var tenant = await currentTenantProvider.TryGetAsync();
        if (tenant != null)
        {
            tenantContextAccessor.Set(tenant);
        }

        await next(context);
    }
}