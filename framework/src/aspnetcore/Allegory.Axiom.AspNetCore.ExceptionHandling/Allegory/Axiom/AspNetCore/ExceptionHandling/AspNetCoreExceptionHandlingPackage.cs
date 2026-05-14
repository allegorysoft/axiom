using System.Net;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Exceptions;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.AspNetCore.ExceptionHandling;

internal sealed class AspNetCoreExceptionHandlingPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddPostConfigureAction(ConfigureExceptionHandling);

        builder.Services.Configure<AspNetCoreExceptionHandlerOptions>(o =>
        {
            o.AddStatusCode<AuthorizationException>(HttpStatusCode.Forbidden);
            o.AddStatusCode<BusinessException>(HttpStatusCode.Conflict);
            o.AddStatusCode<NotFoundException>(HttpStatusCode.NotFound);

            o.AddLogLevel<AuthorizationException>(LogLevel.Warning);
        });

        return Task.CompletedTask;
    }

    private static void ConfigureExceptionHandling(IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<AxiomExceptionHandler>();
    }
}