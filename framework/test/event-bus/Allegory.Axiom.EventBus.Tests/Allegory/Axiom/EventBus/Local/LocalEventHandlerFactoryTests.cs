using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus.Local;

public class LocalEventHandlerFactoryTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected LocalEventHandlerFactory Factory => fixture.Service<LocalEventHandlerFactory>();

    [Fact]
    public void ShouldContainHandlersForRegisteredEvent()
    {
        Factory.Handlers.ContainsKey(typeof(TestEvent)).ShouldBeTrue();
        Factory.Handlers.ContainsKey(typeof(OrderedTestEvent)).ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotContainHandlerForUnregisteredEvent()
    {
        Factory.Handlers.ContainsKey(typeof(UnhandledTestEvent)).ShouldBeFalse();
    }

    [Fact]
    public void ShouldContainAllHandlersForEvent()
    {
        Factory.Handlers[typeof(TestEvent)].Length.ShouldBe(2);
        Factory.Handlers[typeof(OrderedTestEvent)].Length.ShouldBe(2);
    }

    [Fact]
    public void ShouldOrderHandlersByEventOrderAttribute()
    {
        var handlers = Factory.Handlers[typeof(OrderedTestEvent)];

        handlers.Length.ShouldBe(2);
        handlers[0].ShouldBeOfType<ServiceEventHandler<OrderedTestEvent>>()
            .Service.GetType().ShouldBe(typeof(OrderedTestEventHandlerFirst));
        handlers[1].ShouldBeOfType<ServiceEventHandler<OrderedTestEvent>>()
            .Service.GetType().ShouldBe(typeof(OrderedTestEventHandlerSecond));
    }
}

file record TestEvent;

file record UnhandledTestEvent;

file record OrderedTestEvent
{
    public List<Type> Items { get; } = [];
}

file class TestEventHandler1 : ILocalEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent payload) => Task.CompletedTask;
}

file class TestEventHandler2 : ILocalEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent payload) => Task.CompletedTask;
}

[EventOrder(2)]
file class OrderedTestEventHandlerSecond : ILocalEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload)
    {
        payload.Items.Add(typeof(OrderedTestEventHandlerSecond));
        return Task.CompletedTask;
    }
}

[EventOrder(1)]
file class OrderedTestEventHandlerFirst : ILocalEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload)
    {
        payload.Items.Add(typeof(OrderedTestEventHandlerFirst));
        return Task.CompletedTask;
    }
}