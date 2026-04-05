using System.Diagnostics;
using System.Threading;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkManager : IUnitOfWorkManager
{
    private static readonly AsyncLocal<IUnitOfWork?> CurrentAsyncLocal = new();
    public static IUnitOfWork? Current
    {
        get => CurrentAsyncLocal.Value;
        internal set => CurrentAsyncLocal.Value = value;
    }
    private static readonly ActivitySource ActivitySource = new("Allegory.Axiom.UnitOfWork");

    public IUnitOfWork Begin()
    {
        // Transient Disposable is bad design
        var unitOfWork = new UnitOfWork();

        unitOfWork.Parent = Current;
        unitOfWork.Activity = ActivitySource.StartActivity();
        unitOfWork.Activity?.AddTag("id", unitOfWork.Id);

        CurrentAsyncLocal.Value = unitOfWork;

        return unitOfWork;
    }
}