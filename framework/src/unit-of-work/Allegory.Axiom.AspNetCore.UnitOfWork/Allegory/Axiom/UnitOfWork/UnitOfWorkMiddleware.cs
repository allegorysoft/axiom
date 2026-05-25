using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkMiddleware
{
    protected static readonly UnitOfWorkOptions SuppressedTransaction = new(UnitOfWorkTransactionBehavior.Suppress);

    public UnitOfWorkMiddleware(
        RequestDelegate next,
        IUnitOfWorkManager manager,
        IOptions<AspNetCoreUnitOfWorkOptions> options)
    {
        Next = next;
        Manager = manager;
        Options = options.Value;

        options.Value.OptionsSelector ??=
            static context => HttpMethods.IsGet(context.Request.Method)
                ? SuppressedTransaction
                : null;
    }

    protected RequestDelegate Next { get; }
    protected IUnitOfWorkManager Manager { get; }
    protected AspNetCoreUnitOfWorkOptions Options { get; }

    public virtual async Task InvokeAsync(HttpContext context)
    {
        var option = Options.OptionsSelector!(context);
        await using var _ = Manager.Begin(option);// Complete handled by action/endpoint filters
        await Next(context);
    }
}