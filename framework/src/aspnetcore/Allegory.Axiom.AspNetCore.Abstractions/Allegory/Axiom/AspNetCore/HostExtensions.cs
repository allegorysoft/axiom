using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.AspNetCore;

public static class HostExtensions
{
    extension(IHost host)
    {
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
                    $"Host type '{host.GetType().FullName}' cannot be cast to IApplicationBuilder.");
            }

            return builder;
        }
    }
}