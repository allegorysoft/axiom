using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus.Local;

public class LocalEventHandlerManagerTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected LocalEventHandlerManager Manager => fixture.Service<LocalEventHandlerManager>();

    [Fact]
    public void ShouldContainHandlersForRegisteredEvent()
    {
        Manager.Handlers.ContainsKey(typeof(TestEvent)).ShouldBeTrue();
        Manager.Handlers.ContainsKey(typeof(OrderedTestEvent)).ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotContainHandlerForUnregisteredEvent()
    {
        Manager.Handlers.ContainsKey(typeof(UnhandledTestEvent)).ShouldBeFalse();
    }

    [Fact]
    public void ShouldContainAllHandlersForEvent()
    {
        Manager.Handlers[typeof(TestEvent)].Length.ShouldBe(2);
        Manager.Handlers[typeof(OrderedTestEvent)].Length.ShouldBe(2);
    }

    [Fact]
    public void ShouldOrderHandlersByEventOrderAttribute()
    {
        var handlers = Manager.Handlers[typeof(OrderedTestEvent)];

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