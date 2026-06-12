namespace Allegory.Axiom.EventBus.Distributed;

public enum DistributedMessagePublishMode
{
    Immediate,
    OnUnitOfWorkComplete,
    Outbox
}