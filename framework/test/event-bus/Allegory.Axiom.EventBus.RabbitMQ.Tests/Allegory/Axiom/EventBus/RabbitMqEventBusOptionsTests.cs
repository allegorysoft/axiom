using Allegory.Axiom.RabbitMQ;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus;

public class RabbitMqEventBusOptionsTests
{
    [Fact]
    public void ShouldDefaultConnectionNameToDefault()
    {
        var options = new RabbitMqEventBusOptions();

        options.ConnectionName.ShouldBe(RabbitMqOptions.DefaultConnectionName);
    }
}

public class RabbitMqEventBusQueueOptionsTests
{
    [Fact]
    public void ShouldReturnDefaultWhenQueueNotConfigured()
    {
        var options = new RabbitMqEventBusQueueOptions();

        var result = options.Get("unknown-queue");

        result.ShouldBeSameAs(options.Default);
    }

    [Fact]
    public void ShouldReturnPerQueueOptionsWhenConfigured()
    {
        var options = new RabbitMqEventBusQueueOptions();
        var queueOption = new RabbitMqEventBusQueueOption {PrefetchCount = 5};
        options.PerQueue["orders"] = queueOption;

        var result = options.Get("orders");

        result.ShouldBeSameAs(queueOption);
    }
}