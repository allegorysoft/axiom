using Microsoft.AspNetCore.Builder;

namespace Allegory.Axiom.MultiTenancy;

public static class MiddlewareExtensions
{
    extension(IApplicationBuilder applicationBuilder)
    {
        public IApplicationBuilder UseMultiTenancy()
        {
            return applicationBuilder.UseMiddleware<MultiTenancyMiddleware>();
        }
    }
}