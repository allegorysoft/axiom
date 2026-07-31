using System;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkEndpointFilter : IEndpointFilter, ISingletonService
{
    public UnitOfWorkEndpointFilter(
        IUnitOfWorkManager manager,
        IOptions<AspNetCoreUnitOfWorkOptions> options)
    {
        Manager = manager;
        Options = options.Value;

        options.Value.OptionsSelector ??=
            static context => HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsQuery(context.Request.Method)
                ? UnitOfWorkOptions.SuppressedTransaction
                : null;
    }

    protected IUnitOfWorkManager Manager { get; }
    protected AspNetCoreUnitOfWorkOptions Options { get; }

    public virtual async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var option = Options.OptionsSelector!(context.HttpContext);
        await using var uow = Manager.Begin(
            option,
            serviceProvider: context.HttpContext.RequestServices,
            cancellationToken: context.HttpContext.RequestAborted);

        object? result;
        try
        {
            result = await next(context);
        }
        catch (Exception e)
        {
            await uow.TryRollbackAsync(e, cancellationToken: CancellationToken.None);
            throw;
        }

        await uow.TryCompleteAsync(cancellationToken: CancellationToken.None);
        return result;
    }
}