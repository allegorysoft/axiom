using System.Diagnostics;
using System.Threading;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkManager : IUnitOfWorkManager
{
    internal static readonly AsyncLocal<IUnitOfWork?> CurrentUnitOfWork = new();
    private static readonly ActivitySource ActivitySource = new("Allegory.Axiom.UnitOfWork");

    public virtual IUnitOfWork? Current => CurrentUnitOfWork.Value;

    public virtual IUnitOfWork Begin()
    {
        // Transient Disposable is bad design
        var unitOfWork = new UnitOfWork();

        unitOfWork.Parent = Current;
        unitOfWork.Activity = ActivitySource.StartActivity();
        unitOfWork.Activity?.AddTag("id", unitOfWork.Id);

        CurrentUnitOfWork.Value = unitOfWork;

        return unitOfWork;
    }
}