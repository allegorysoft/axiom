namespace Allegory.Axiom.EventBus.Distributed;

public enum QueueTopology
{
    Single,
    PerEventType,
    PerHandler,
    PerAssembly
}