using Allegory.Axiom.EventBus;
using Allegory.Axiom.UnitOfWork;
using OpenTelemetry.Trace;

namespace Allegory.Axiom.OpenTelemetry;

public static class TracerProviderBuilderExtensions
{
    extension(TracerProviderBuilder builder)
    {
        public TracerProviderBuilder AddAxiomInstrumentation()
        {
            return builder
                .AddEventBusInstrumentation()
                .AddUnitOfWorkInstrumentation();
        }

        public TracerProviderBuilder AddEventBusInstrumentation()
        {
            return builder.AddSource(EventBusActivity.Name);
        }

        public TracerProviderBuilder AddUnitOfWorkInstrumentation()
        {
            return builder.AddSource(UnitOfWorkActivity.Name);
        }
    }
}