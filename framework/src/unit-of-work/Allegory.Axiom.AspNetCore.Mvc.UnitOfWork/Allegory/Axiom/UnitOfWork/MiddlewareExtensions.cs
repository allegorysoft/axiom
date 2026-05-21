using Microsoft.AspNetCore.Builder;

namespace Allegory.Axiom.UnitOfWork;

public static class MiddlewareExtensions
{
    extension(IApplicationBuilder applicationBuilder)
    {
        public IApplicationBuilder UseUnitOfWork()
        {
            return applicationBuilder.UseMiddleware<UnitOfWorkMiddleware>();
        }
    }
}