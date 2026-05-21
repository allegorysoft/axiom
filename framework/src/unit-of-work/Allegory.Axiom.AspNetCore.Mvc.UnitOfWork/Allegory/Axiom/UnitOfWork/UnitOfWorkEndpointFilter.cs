using System;
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

        object? result;
        try
        {
            result = await next(context);
        }
        catch (Exception e)
        {
            await uow.TryRollbackAsync(e);
            throw;
        }

        await uow.TryCompleteAsync(context.HttpContext.RequestAborted);
        return result;
    }
}