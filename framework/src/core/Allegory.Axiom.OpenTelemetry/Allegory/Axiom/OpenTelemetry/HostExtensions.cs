using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

namespace Allegory.Axiom.OpenTelemetry;

public static class HostExtensions
{
    extension(IHost host)
    {
        public IHost EnsureTracingStarted()
        {
            host.Services.GetRequiredService<TracerProvider>();
            return host;
        }
    }
}