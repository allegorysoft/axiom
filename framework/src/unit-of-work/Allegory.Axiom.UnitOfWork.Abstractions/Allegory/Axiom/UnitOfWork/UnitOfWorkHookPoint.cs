namespace Allegory.Axiom.UnitOfWork;

public enum UnitOfWorkHookPoint
{
    BeforeSave,
    AfterSave,
    BeforeComplete,
    AfterComplete,
    BeforeRollback,
    AfterRollback
}