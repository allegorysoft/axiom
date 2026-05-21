using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkActionFilter(IUnitOfWorkManager manager) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Middleware already disposes
        // However early dispose is better for releasing transactional connections
        await using var uow = manager.Current;

        if (uow == null)
        {
            await next();
            return;
        }

        var result = await next();

        if (result.Exception == null || result.ExceptionHandled)
        {
            await uow.TryCompleteAsync(result.HttpContext.RequestAborted);
        }
        else
        {
            await uow.TryRollbackAsync(result.Exception, result.HttpContext.RequestAborted);
        }
    }
}