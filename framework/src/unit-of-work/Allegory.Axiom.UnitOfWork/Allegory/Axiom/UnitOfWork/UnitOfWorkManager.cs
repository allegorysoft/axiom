using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkManager(IOptions<UnitOfWorkOptions> options) : IUnitOfWorkManager
{
    internal static readonly AsyncLocal<IUnitOfWork?> CurrentUnitOfWork = new();
    private static readonly ActivitySource ActivitySource = new("Allegory.Axiom.UnitOfWork");

    public virtual IUnitOfWork? Current => CurrentUnitOfWork.Value;
    protected virtual UnitOfWorkOptions Options { get; } = options.Value;

    public virtual IUnitOfWork Begin(UnitOfWorkOptions? options = null)
    {
        var option = GetUnitOfWorkOptions(options);
        var unitOfWork = CreateUnitOfWork(option);
        CurrentUnitOfWork.Value = unitOfWork;

        return unitOfWork;
    }

    protected virtual IUnitOfWork CreateUnitOfWork(UnitOfWorkOptions options)
    {
        UnitOfWorkBase unitOfWork;

        if (Current == null || 
            options.TransactionBehavior == UnitOfWorkTransactionBehavior.RequiresNew ||
            options.TransactionBehavior == UnitOfWorkTransactionBehavior.Suppress)
        {
            unitOfWork = new UnitOfWork(options);
        }
        else
        {
            unitOfWork = new ChildUnitOfWork(options);
        }

        unitOfWork.Parent = Current;
        unitOfWork.Activity = ActivitySource.StartActivity();
        unitOfWork.Activity?.AddTag("id", unitOfWork.Id);

        return unitOfWork;
    }

    protected virtual UnitOfWorkOptions GetUnitOfWorkOptions(UnitOfWorkOptions? preferred = null)
    {
        if (preferred == null)
        {
            return Options;
        }

        preferred.Timeout ??= Options.Timeout;
        preferred.IsolationLevel ??= Options.IsolationLevel;

        return preferred;
    }
}