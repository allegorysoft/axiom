using Allegory.Axiom.EventBus.Distributed;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus;

public class RabbitMqEventBusOptionsExtensionsTests
{
    [Fact]
    public void ShouldCreateOptionsLazilyOnFirstAccess()
    {
        var options = new DistributedEventBusOptions();

        options.RabbitMq.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldReturnSameInstanceOnSubsequentAccess()
    {
        var options = new DistributedEventBusOptions();

        var first = options.RabbitMq;
        var second = options.RabbitMq;

        second.ShouldBeSameAs(first);
    }

    [Fact]
    public void ShouldAllowSettingOptionsExplicitly()
    {
        var options = new DistributedEventBusOptions();
        var custom = new RabbitMqEventBusOptions {ExchangeName = "custom-exchange"};

        options.RabbitMq = custom;

        options.RabbitMq.ShouldBeSameAs(custom);
        options.RabbitMq.ExchangeName.ShouldBe("custom-exchange");
    }

    [Fact]
    public void ShouldNotShareInstanceBetweenDifferentOptionsObjects()
    {
        var options1 = new DistributedEventBusOptions();
        var options2 = new DistributedEventBusOptions();

        options1.RabbitMq.ExchangeName = "options1-exchange";

        options2.RabbitMq.ExchangeName.ShouldBeNull();
    }
}