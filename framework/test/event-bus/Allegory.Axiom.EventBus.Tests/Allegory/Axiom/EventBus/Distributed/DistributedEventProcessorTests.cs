using System;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Exceptions;
using Allegory.Axiom.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventProcessorTests(
    DistributedEventProcessorFixture fixture)
    : IClassFixture<DistributedEventProcessorFixture>
{
    protected DistributedEventProcessor Processor => fixture.Service<DistributedEventProcessor>();
    protected ITenantContextAccessor TenantContextAccessor => fixture.Service<ITenantContextAccessor>();

    protected EventQueueEntry Entry => fixture.Service<DistributedEventHandlerManager>()
        .Queues.Single().Value
        .Events[typeof(TestEvent).FullName!];

    [Fact]
    public async Task ShouldTrackPendingProcessesWhileInFlight()
    {
        var pendingCounter = await Processor.ProcessAsync(
            "test-queue",
            Entry,
            Guid.NewGuid(),
            new TestEvent(),
            cancellationToken: TestContext.Current.CancellationToken);

        Processor.PendingProcesses.ShouldBe(1);
        pendingCounter.Dispose();
        Processor.PendingProcesses.ShouldBe(0);
    }

    [Fact]
    public async Task ShouldDecrementPendingProcessWhenHandlerThrows()
    {
        await Should.ThrowAsync<InvalidCastException>(async () =>
        {
            await Processor.ProcessAsync(
                "test-queue",
                Entry,
                Guid.NewGuid(),
                1, // Cannot cast int to TestEvent
                cancellationToken: TestContext.Current.CancellationToken);
        });
        
        Processor.PendingProcesses.ShouldBe(0);
    }

    [Fact]
    public async Task ShouldSetTenantContextWhenTenantIdProvided()
    {
        var handler = fixture.Service<EventHandler1>();

        using var counter = await Processor.ProcessAsync(
            "test-queue",
            Entry,
            Guid.NewGuid(),
            new TestEvent(),
            tenantId: fixture.Tenant1.Id,
            cancellationToken: TestContext.Current.CancellationToken);

        handler.CapturedTenantId.ShouldBe(fixture.Tenant1.Id);
    }

    [Fact]
    public async Task ShouldLeaveTenantContextNullWhenNoTenantIdProvided()
    {
        var handler = fixture.Service<EventHandler1>();

        using var counter = await Processor.ProcessAsync(
            "test-queue",
            Entry,
            Guid.NewGuid(),
            new TestEvent(),
            tenantId: null,
            cancellationToken: TestContext.Current.CancellationToken);

        handler.CapturedTenantId.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldClearTenantContextAfterProcessingCompletes()
    {
        var handler = fixture.Service<EventHandler1>();

        using var counter = await Processor.ProcessAsync(
            "test-queue",
            Entry,
            Guid.NewGuid(),
            new TestEvent(),
            tenantId: fixture.Tenant1.Id,
            cancellationToken: TestContext.Current.CancellationToken);

        handler.CapturedTenantId.ShouldBe(fixture.Tenant1.Id);
        // Assert ambient context doesn't leak past this processing call
        TenantContextAccessor.Current.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldNotLeakTenantContextBetweenSequentialProcessCalls()
    {
        var handler = fixture.Service<EventHandler1>();

        using (await Processor.ProcessAsync(
                   "test-queue", Entry, Guid.NewGuid(), new TestEvent(),
                   tenantId: fixture.Tenant1.Id,
                   cancellationToken: TestContext.Current.CancellationToken))
        {
        }

        handler.CapturedTenantId.ShouldBe(fixture.Tenant1.Id);

        using var counter = await Processor.ProcessAsync(
            "test-queue", Entry, Guid.NewGuid(), new TestEvent(),
            tenantId: null,
            cancellationToken: TestContext.Current.CancellationToken);

        handler.CapturedTenantId.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldThrowWhenSpecifiedTenantIdNotFoundInStore()
    {
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
        {
            using var counter = await Processor.ProcessAsync(
                "test-queue",
                Entry,
                Guid.NewGuid(),
                new TestEvent(),
                tenantId: Guid.NewGuid(),
                cancellationToken: TestContext.Current.CancellationToken);
        });

        exception.Code.ShouldBe(MultiTenancyExceptionCodes.TenantNotFound);
    }
}

public class DistributedEventProcessorFixture : IntegrationTest
{
    public TenantContext Tenant1 { get; } = new(Guid.NewGuid(), "t-1", "T-1");

    protected override Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        var fakeTenantStore = Substitute.For<ITenantStore>();
        fakeTenantStore.FindAsync(Tenant1.Id).Returns(Tenant1);
        builder.Services.Replace(ServiceDescriptor.Singleton(fakeTenantStore));

        return Task.CompletedTask;
    }
}

file class TestEvent();

file class EventHandler1(ITenantContextAccessor accessor) : IDistributedEventHandler<TestEvent>
{
    public Guid? CapturedTenantId { get; protected set; }

    public Task HandleAsync(TestEvent payload, EventContext context)
    {
        CapturedTenantId = accessor.Current?.Id;

        return Task.CompletedTask;
    }
}