namespace Allegory.Axiom.UnitOfWork;

public enum UnitOfWorkHookPriority : byte
{
    Highest = 0,
    High = 50,
    Normal = 100,
    Low = 150,
    Lowest = 200,
}