namespace Allegory.Axiom.EventBus.Distributed;

public enum QueueTopology
{
    Single,
    PerMessageType,
    PerHandler,
    PerAssembly
}