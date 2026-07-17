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
            // https://github.com/allegorysoft/axiom/issues/83
            host.Services.GetRequiredService<TracerProvider>();
            return host;
        }
    }
}