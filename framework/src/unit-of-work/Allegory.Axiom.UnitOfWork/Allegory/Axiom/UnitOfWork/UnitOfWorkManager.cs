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

    protected virtual IUnitOfWork CreateUnitOfWork(UnitOfWorkOptions options)
    {
        return ShouldCreateRoot(options) ? CreateRootUnitOfWork(options) : new ChildUnitOfWork(options, Current!);
    }

    protected virtual bool ShouldCreateRoot(UnitOfWorkOptions options)
    {
        if (Current == null)
        {
            return true;
        }

        if (options.TransactionBehavior == UnitOfWorkTransactionBehavior.RequiresNew)
        {
            return true;
        }

        if (Current.Options.TransactionBehavior == options.TransactionBehavior ||
            Current.Options.TransactionBehavior == UnitOfWorkTransactionBehavior.RequiresNew &&
            options.TransactionBehavior == UnitOfWorkTransactionBehavior.Required)
        {
            return false;
        }

        return true;
    }

    protected virtual IUnitOfWork CreateRootUnitOfWork(UnitOfWorkOptions options)
    {
        var unitOfWork = new UnitOfWork(options, Current);
        unitOfWork.Activity = ActivitySource.StartActivity();
        unitOfWork.Activity?.AddTag("id", unitOfWork.Id);
        return unitOfWork;
    }
}