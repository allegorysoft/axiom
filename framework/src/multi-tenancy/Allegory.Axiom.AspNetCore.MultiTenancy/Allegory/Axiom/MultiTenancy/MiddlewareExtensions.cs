using Microsoft.AspNetCore.Builder;

namespace Allegory.Axiom.MultiTenancy;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MultiTenancyMiddleware>();
    }
}