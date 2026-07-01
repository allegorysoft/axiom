using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventHandlerManagerTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected DistributedEventHandlerManager Manager => fixture.Service<DistributedEventHandlerManager>();

    [Fact]
    public void Test()
    {
        
    }
}

file record TestEvent;

file record UnhandledTestEvent;

file record OrderedTestEvent
{
    public List<Type> Items { get; } = [];
}

file class TestEventHandler1 : IDistributedEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent payload) => Task.CompletedTask;
}

file class TestEventHandler2 : IDistributedEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent payload) => Task.CompletedTask;
}

[EventOrder(2)]
file class OrderedTestEventHandlerSecond : IDistributedEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload)
    {
        payload.Items.Add(typeof(OrderedTestEventHandlerSecond));
        return Task.CompletedTask;
    }
}

[EventOrder(1)]
file class OrderedTestEventHandlerFirst : IDistributedEventHandler<OrderedTestEvent>
{
    public Task HandleAsync(OrderedTestEvent payload)
    {
        payload.Items.Add(typeof(OrderedTestEventHandlerFirst));
        return Task.CompletedTask;
    }
}