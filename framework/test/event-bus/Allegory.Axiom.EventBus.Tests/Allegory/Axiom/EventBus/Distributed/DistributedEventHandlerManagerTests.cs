using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.Extensions;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventHandlerManagerTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected DistributedEventHandlerManager Manager => fixture.Service<DistributedEventHandlerManager>();

    [Fact]
    public async Task Test()
    {
        var provider = await fixture.CreateServiceProviderAsync(c =>
        {
            c.Services.Configure<DistributedEventBusOptions>(options =>
            {
                options.Queue.Topology = QueueTopology.PerMessageType;
            });
        });

        var manager = provider.GetRequiredService<DistributedEventHandlerManager>();
    }
}

public class Event1{}
public class Event2 {}

public class EventHandler1 : IDistributedEventHandler<Event1>
{
    public Task HandleAsync(Event1 payload) =>  Task.CompletedTask;
}

[EventOrder(-1)]
public class EventHandler2 : IDistributedEventHandler<Event2>, IDistributedEventHandler<Event1>
{
    public Task HandleAsync(Event2 payload) =>  Task.CompletedTask;
    public Task HandleAsync(Event1 payload) =>   Task.CompletedTask;
}

file record TestEvent;

file record UnhandledTestEvent;


file class TestEventHandler1 : IDistributedEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent payload) => Task.CompletedTask;
}

file class TestEventHandler2 : IDistributedEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent payload) => Task.CompletedTask;
}