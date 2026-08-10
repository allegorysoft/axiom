namespace Allegory.Axiom.UnitOfWork;

public enum UnitOfWorkHookPoint : byte
{
    BeforeComplete,
    AfterComplete,
    BeforeRollback,
    AfterRollback
}