using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkMiddleware(
    RequestDelegate next,
    IUnitOfWorkManager manager,
    IOptions<AspNetCoreUnitOfWorkOptions> options)
{
    protected static readonly UnitOfWorkOptions SuppressedTransaction = new(UnitOfWorkTransactionBehavior.Suppress);
    protected AspNetCoreUnitOfWorkOptions Options { get; } = options.Value;

    public virtual async Task InvokeAsync(HttpContext context)
    {
        var option = GetUnitOfWorkOptions(context);

        await using var unitOfWork = manager.Begin(option);
        await next(context);
        await unitOfWork.CompleteAsync();
    }

    protected virtual UnitOfWorkOptions? GetUnitOfWorkOptions(HttpContext context)
    {
        if (Options.OptionsSelector != null)
        {
            return Options.OptionsSelector(context);
        }

        return HttpMethods.IsGet(context.Request.Method) ? SuppressedTransaction : null;
    }
}