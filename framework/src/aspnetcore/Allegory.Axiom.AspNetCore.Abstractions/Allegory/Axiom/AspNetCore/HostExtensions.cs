using System;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.AspNetCore;

public static class HostExtensions
{
    internal static readonly ConditionalWeakTable<IHost, ExtraProperties> HostProperties = new();

    extension(IHost host)
    {
        public WebApplication GetWebApplication()
        {
            if (host is not WebApplication app)
            {
                throw new InvalidOperationException(
                    $"Host type '{host.GetType().FullName}' cannot be cast to WebApplication.");
            }

            return app;
        }

        public IApplicationBuilder GetApplicationBuilder()
        {
            if (host is not IApplicationBuilder builder)
            {
                throw new InvalidOperationException(
                    $"Host type '{host.GetType().FullName}' cannot be cast to IApplicationBuilder.");
            }

            return builder;
        }

        public IEndpointRouteBuilder GetEndpointRouteBuilder()
        {
            if (host is not IEndpointRouteBuilder builder)
            {
                throw new InvalidOperationException(
                    $"Host type '{host.GetType().FullName}' cannot be cast to IEndpointRouteBuilder.");
            }

            return builder;
        }

        public RouteGroupBuilder GetDefaultRouteGroupBuilder()
        {
            // https://github.com/dotnet/aspnetcore/issues/43237
            // Default route group ensures global endpoint filters are applied across all endpoints.

            var hostProperties = HostProperties.GetOrCreateValue(host);

            if (hostProperties.DefaultRouteGroupBuilder == null)
            {
                var builder = host.GetEndpointRouteBuilder();
                hostProperties.DefaultRouteGroupBuilder = builder.MapGroup(string.Empty);
            }

            return hostProperties.DefaultRouteGroupBuilder;
        }
    }

    internal class ExtraProperties
    {
        public RouteGroupBuilder? DefaultRouteGroupBuilder { get; set; }
    }
}