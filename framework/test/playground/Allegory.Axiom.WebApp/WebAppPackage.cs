using System.Threading.Tasks;
using Allegory.Axiom.AspNetCore;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal sealed class WebAppPackage : IConfigureApplication, IInitializeApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        // Configure open telemetry
        // Use rabbitmq distributed event bus
        builder.Services.AddOpenApi();

        return Task.CompletedTask;
    }

    public static Task InitializeAsync(IHost host)
    {
        var app = host.GetWebApplication();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseExceptionHandler();
        app.UseUnitOfWork();
        app.UseAuthorization();
        app.UseMultiTenancy();
        app.UseAuthorization();

        return Task.CompletedTask;
    }
}