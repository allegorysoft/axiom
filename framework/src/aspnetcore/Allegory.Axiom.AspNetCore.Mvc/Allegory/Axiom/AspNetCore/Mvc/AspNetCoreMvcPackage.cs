using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.AspNetCore.Mvc;

internal sealed class AspNetCoreMvcPackage : IConfigureApplication, IInitializeApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        var mvcBuilder = builder.Services.AddControllers();
        builder.AddBuilder(mvcBuilder);

        return Task.CompletedTask;
    }

    public static Task InitializeAsync(IHost host)
    {
        var defaultRouteBuilder = host.GetDefaultRouteGroupBuilder();
        var conventionBuilder = defaultRouteBuilder.MapControllers();
        host.AddBuilder(conventionBuilder);

        return Task.CompletedTask;
    }
}