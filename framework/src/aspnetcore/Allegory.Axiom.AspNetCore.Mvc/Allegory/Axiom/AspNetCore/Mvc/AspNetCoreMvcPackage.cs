using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.AspNetCore.Mvc;

internal sealed class AspNetCoreMvcPackage : IConfigureApplication, IInitializeApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddPostConfigureAction(ConfigurePackage);

        return Task.CompletedTask;
    }

    private static void ConfigurePackage(IServiceCollection services)
    {
        var mvcBuilder = services.AddControllers();
        services.ExecuteBuilderActions(mvcBuilder);
    }

    public static Task InitializeAsync(IHost host)
    {
        var endpoint = host.GetEndpointRouteBuilder();
        endpoint.MapControllers();

        return Task.CompletedTask;
    }
}