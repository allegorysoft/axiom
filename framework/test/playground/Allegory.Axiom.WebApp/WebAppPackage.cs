using System;
using System.Threading.Tasks;
using Allegory.Axiom.AspNetCore;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.OpenTelemetry;
using Allegory.Axiom.UnitOfWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

internal sealed class WebAppPackage : IConfigureApplication, IInitializeApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        ConfigureOpenTelemetry(builder);

        return Task.CompletedTask;
    }

    private static void ConfigureOpenTelemetry(IHostApplicationBuilder builder)
    {
        /*
        podman run -d \
            --name aspire-dashboard \
            -p 18888:18888 \
            -p 18889:18889 \
            -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true \
            mcr.microsoft.com/dotnet/aspire-dashboard:latest

        podman run -d \
            --name rabbitmq \
            -p 5672:5672 \
            -p 15672:15672 \
            -e RABBITMQ_DEFAULT_USER=guest \
            -e RABBITMQ_DEFAULT_PASS=guest \
            docker.io/rabbitmq:4-management

        podman run -d \
            --name redis-stack \
            -p 6379:6379 \
            -p 8001:8001 \
            docker.io/redis/redis-stack:latest
         */

        var exporterOptionsAction = static (OtlpExporterOptions o) =>
        {
            o.Endpoint = new Uri("http://localhost:18889");
            o.Protocol = OtlpExportProtocol.Grpc;
        };

        var resource = ResourceBuilder.CreateDefault()
            .AddService("WebApp", serviceVersion: "1.0.0");

        builder.Services
            .AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .SetResourceBuilder(resource)
                .AddAxiomInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(exporterOptionsAction))
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(resource)
                //.AddMeter("TelemetryName")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(exporterOptionsAction))
            .WithLogging(logging => logging
                .SetResourceBuilder(resource)
                .AddOtlpExporter(exporterOptionsAction));
    }

    public static Task InitializeAsync(IHost host)
    {
        var app = host.GetWebApplication();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseMultiTenancy();
        app.UseAuthorization();

        return Task.CompletedTask;
    }
}