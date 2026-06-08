namespace Allegory.Axiom.EventBus;

public enum DistributedMessagePublishMode
{
    Immediate,
    OnUnitOfWorkComplete,
    Outbox
}