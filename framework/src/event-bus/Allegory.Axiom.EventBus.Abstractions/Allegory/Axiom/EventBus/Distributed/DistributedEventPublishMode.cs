namespace Allegory.Axiom.EventBus.Distributed;

public enum DistributedEventPublishMode
{
    Immediate,
    OnUnitOfWorkComplete,
    Outbox,
    Auto
}