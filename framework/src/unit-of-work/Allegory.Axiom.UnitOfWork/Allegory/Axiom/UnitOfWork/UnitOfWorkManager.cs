using System.Diagnostics;
using System.Threading;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Threading;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkManager(IOptions<UnitOfWorkOptions> options) : IUnitOfWorkManager, ISingletonService
{
    protected internal static readonly AsyncLocal<AsyncLocalContext<IUnitOfWork>?> CurrentUnitOfWork = new();

    public virtual IUnitOfWork? Current => CurrentUnitOfWork.Value?.Context;
    protected virtual UnitOfWorkOptions Options { get; } = options.Value;

    public virtual IUnitOfWork Begin(UnitOfWorkOptions? options = null)
    {
        var unitOfWork = CreateUnitOfWork(GetUnitOfWorkOptions(options));

        if (CurrentUnitOfWork.Value == null)
        {
            CurrentUnitOfWork.Value = new AsyncLocalContext<IUnitOfWork>(unitOfWork);
        }
        else
        {
            CurrentUnitOfWork.Value.Context = unitOfWork;
        }

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
        return ShouldCreateRoot(options) ? CreateRootUnitOfWork(options) : new ChildUnitOfWork(Current!);
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
        var unitOfWork = new UnitOfWork(options);
        unitOfWork.Parent = Current;
        unitOfWork.Activity = UnitOfWorkActivity.Source.StartActivity(name: "UnitOfWork");

        if (unitOfWork.Activity is not null)
        {
            unitOfWork.Activity.SetTag("uow.id", unitOfWork.Id);
            unitOfWork.Activity.SetTag("uow.transaction_behaviour", options.TransactionBehavior.ToString());
        }

        return unitOfWork;
    }
}