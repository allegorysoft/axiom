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
                options.Queue.Topology = QueueTopology.PerHandler;
            });
        });

        var manager = provider.GetRequiredService<DistributedEventHandlerManager>();

    }
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