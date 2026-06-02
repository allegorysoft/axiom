using System;
using System.Threading.Tasks;
using Xunit;

namespace Allegory.Axiom.EventBus;

public class LocalEventBusTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task Test()
    {
        var eventBus = fixture.Service<ILocalEventBus>();

        eventBus.Subscribe("a", o =>
        {
            dynamic d = o;
            var x = d.a;
            return Task.CompletedTask;
        });

        eventBus.Subscribe("a", o =>
        {
            return Task.CompletedTask;
        });

        eventBus.Subscribe("b", o =>
        {
            return Task.CompletedTask;
        });

        await eventBus.PublishAsync(
            "a",
            new
            {
                a = "nice"
            });
    }
}