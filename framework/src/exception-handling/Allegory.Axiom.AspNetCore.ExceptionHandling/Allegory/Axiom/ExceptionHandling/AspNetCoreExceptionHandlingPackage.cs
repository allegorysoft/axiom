using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.ExceptionHandling;

internal sealed class AspNetCoreExceptionHandlingPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddPostConfigureAction(ConfigureExceptionHandling);

        return Task.CompletedTask;
    }

    private static void ConfigureExceptionHandling(IServiceCollection services)
    {
        services.AddExceptionHandler<AxiomExceptionHandler>();
        services.AddProblemDetails();
    }
}