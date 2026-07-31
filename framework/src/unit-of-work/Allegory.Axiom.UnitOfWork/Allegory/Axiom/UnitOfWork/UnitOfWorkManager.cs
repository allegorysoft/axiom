using System;
using System.Threading;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkManager(
    IOptions<UnitOfWorkOptions> options,
    ITenantContextAccessor tenantContextAccessor,
    IServiceScopeFactory  serviceScopeFactory) 
    : IUnitOfWorkManager, ISingletonService
{
    protected internal static readonly AsyncLocal<AsyncLocalContext<IUnitOfWork>?> CurrentUnitOfWork = new();

    public virtual IUnitOfWork? Current => CurrentUnitOfWork.Value?.Context;
    public virtual IUnitOfWork RequiredCurrent => Current ?? throw new InvalidOperationException(
        "No ambient unit of work found. Ensure a unit of work scope has been started before accessing this property");
    protected UnitOfWorkOptions Options { get; } = options.Value;
    protected ITenantContextAccessor TenantContextAccessor { get; } = tenantContextAccessor;
    protected IServiceScopeFactory ServiceScopeFactory { get; } = serviceScopeFactory;

    public virtual IUnitOfWork Begin(
        UnitOfWorkOptions? options = null,
        IServiceProvider? serviceProvider = null,
        CancellationToken cancellationToken = default)
    {
        var unitOfWork = CreateUnitOfWork(GetUnitOfWorkOptions(options), serviceProvider, cancellationToken);

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

    protected virtual IUnitOfWork CreateUnitOfWork(
        UnitOfWorkOptions options,
        IServiceProvider? serviceProvider = null,
        CancellationToken cancellationToken = default)
    {
        if (ShouldCreateRoot(options))
        {
            return CreateRootUnitOfWork(options, serviceProvider, cancellationToken);
        }

        var parent = RequiredCurrent;
        cancellationToken = GetOrCreateCancellationToken(cancellationToken, out var cancellationTokenSource);

        return new ChildUnitOfWork(
            parent,
            serviceProvider ?? parent.ServiceProvider,
            cancellationToken: cancellationToken,
            cancellationTokenSource: cancellationTokenSource);
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

    protected virtual IUnitOfWork CreateRootUnitOfWork(
        UnitOfWorkOptions options,
        IServiceProvider? serviceProvider = null,
        CancellationToken cancellationToken = default)
    {
        serviceProvider = GetOrCreateServiceProvider(serviceProvider, out var asyncServiceScope);
        cancellationToken = GetOrCreateCancellationToken(cancellationToken, out var cancellationTokenSource);

        var unitOfWork = new UnitOfWork(
            options,
            serviceProvider,
            asyncServiceScope: asyncServiceScope,
            cancellationToken: cancellationToken,
            cancellationTokenSource: cancellationTokenSource);
        unitOfWork.Parent = Current;
        unitOfWork.Activity = UnitOfWorkActivity.Source.StartActivity(name: "UnitOfWork");

        if (unitOfWork.Activity is not null)
        {
            unitOfWork.Activity.SetTag("uow.id", unitOfWork.Id);
            unitOfWork.Activity.SetTag("uow.transaction_behaviour", options.TransactionBehavior.ToString());
            unitOfWork.Activity.SetTag("tenant.id", TenantContextAccessor.Current?.Id);
        }

        return unitOfWork;
    }

    protected virtual IServiceProvider GetOrCreateServiceProvider(
        IServiceProvider? serviceProvider,
        out AsyncServiceScope? asyncServiceScope)
    {
        serviceProvider ??= Current?.ServiceProvider;
        asyncServiceScope = null;

        if (serviceProvider == null)
        {
            asyncServiceScope = ServiceScopeFactory.CreateAsyncScope();
            serviceProvider = asyncServiceScope.Value.ServiceProvider;
        }

        return serviceProvider;
    }

    protected virtual CancellationToken GetOrCreateCancellationToken(
        CancellationToken cancellationToken,
        out CancellationTokenSource? cancellationTokenSource)
    {
        cancellationTokenSource = null;
        var parentCancellationToken = Current?.CancellationToken;

        if (!parentCancellationToken.HasValue || parentCancellationToken.Value == CancellationToken.None)
        {
            return cancellationToken;
        }

        if (cancellationToken == CancellationToken.None)
        {
            return parentCancellationToken.Value;
        }

        cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(parentCancellationToken.Value, cancellationToken);

        return cancellationTokenSource.Token;
    }
}