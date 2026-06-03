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

        eventBus.Subscribe("a", static o =>
        {
            return Task.CompletedTask;
        });

        await eventBus.PublishAsync(new OrderCreated(123));
        await eventBus.PublishAsync(
            "a",
            new
            {
                a = "nice"
            });
    }
}

public record OrderCreated(int OrderId);
public record SomeCreated(int OrderId);

public class OrderCreatedHandler : ILocalEventHandler<OrderCreated>
{
    public Task HandleAsync(OrderCreated payload)
    {
        return Task.CompletedTask;
    }
}

public class OrderCreatedHandler2 : ILocalEventHandler<OrderCreated>
{
    public Guid Id { get;  } = Guid.NewGuid();
    public Task HandleAsync(OrderCreated payload)
    {
        return Task.CompletedTask;
    }
}

public class A {}

public class AHanlder : ILocalEventHandler<A>
{
    public Task HandleAsync(A payload)
    {
        return Task.CompletedTask;
    }
}