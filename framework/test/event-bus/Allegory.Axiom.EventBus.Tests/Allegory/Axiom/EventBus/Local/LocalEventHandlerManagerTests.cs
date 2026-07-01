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
        Manager.Handlers.ContainsKey(typeof(TestEvent2)).ShouldBeTrue();
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
        Manager.Handlers[typeof(TestEvent2)].Length.ShouldBe(1);
    }
}

file record TestEvent;

file record TestEvent2;

file record UnhandledTestEvent;

file class EventHandler1 : ILocalEventHandler<TestEvent>
{
    public Task HandleAsync(TestEvent payload) => Task.CompletedTask;
}

file class EventHandler2 : ILocalEventHandler<TestEvent>, ILocalEventHandler<TestEvent2>
{
    public Task HandleAsync(TestEvent payload) => Task.CompletedTask;
    public Task HandleAsync(TestEvent2 payload) => Task.CompletedTask;
}