using System;
using System.Diagnostics;
using System.Threading;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Disposables;

namespace Allegory.Axiom.MultiTenancy;

public class TenantContextAccessor : ITenantContextAccessor, ISingletonService
{
    protected internal static readonly AsyncLocal<TenantContext?> CurrentTenantContext = new();

    public virtual TenantContext? Current => CurrentTenantContext.Value;

    public virtual void Set(TenantContext? current = null)
    {
        Activity.Current?.AddBaggage("tenant.id", current?.Id.ToString());
        // Reduce disposable object allocation
        CurrentTenantContext.Value = current;
    }

    public virtual IDisposable Change(TenantContext? current = null)
    {
        Activity.Current?.AddBaggage("tenant.id", current?.Id.ToString());
        var parent = Current;
        CurrentTenantContext.Value = current;

        return new DisposableDelegate<TenantContext?>(Restore, parent);
    }

    private static void Restore(TenantContext? parent)
    {
        Activity.Current?.AddBaggage("tenant.id", parent?.Id.ToString());
        CurrentTenantContext.Value = parent;
    }
}