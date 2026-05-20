using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkEndpointFilter(IUnitOfWorkManager manager) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        // Middleware already disposes
        // However early dispose is better for releasing transactional connections
        await using var uow = manager.Current;

        if (uow == null)
        {
            return await next(context);
        }

        var result = await next(context);
        await uow.CompleteAsync();
        return result;
    }
}