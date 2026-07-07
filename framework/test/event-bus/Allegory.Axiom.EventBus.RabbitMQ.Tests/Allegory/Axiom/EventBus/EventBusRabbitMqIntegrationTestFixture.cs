using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;

namespace Allegory.Axiom.EventBus;

public class EventBusRabbitMqIntegrationTestFixture : IntegrationTestFixture
{
    public override async ValueTask DisposeAsync()
    {
        var processor = Service<DistributedEventProcessor>();
        await processor.WaitForCompletionAsync();
        await base.DisposeAsync();
    }
}