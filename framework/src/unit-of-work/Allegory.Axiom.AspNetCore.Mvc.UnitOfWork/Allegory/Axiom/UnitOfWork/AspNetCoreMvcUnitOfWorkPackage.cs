using System.Threading.Tasks;
using Allegory.Axiom.AspNetCore;
using Allegory.Axiom.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.UnitOfWork;

internal sealed class AspNetCoreMvcUnitOfWorkPackage : IConfigureApplication, IInitializeApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.AddBuilderAction<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddMvcOptions(mvcOptions =>
            {
                mvcOptions.Filters.Add<UnitOfWorkActionFilter>();
            });
        });

        return Task.CompletedTask;
    }

    public static Task InitializeAsync(IHost host)
    {
        var builder = host.GetDefaultRouteGroupBuilder();
        builder.AddEndpointFilter<UnitOfWorkEndpointFilter>();

        return Task.CompletedTask;
    }
}