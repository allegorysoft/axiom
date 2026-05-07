using System;
using System.Threading;
using Allegory.Axiom.Disposables;

namespace Allegory.Axiom.MultiTenancy;

public class TenantContextAccessor : ITenantContextAccessor
{
    protected internal static readonly AsyncLocal<TenantContext?> CurrentTenantContext = new();

    public virtual TenantContext? Current => CurrentTenantContext.Value;

    public virtual void Set(TenantContext? current = null)
    {
        // Reduce disposable object allocation
        CurrentTenantContext.Value = current;
    }

    public virtual IDisposable Change(TenantContext? current = null)
    {
        var parent = Current;
        CurrentTenantContext.Value = current;

        return new DisposableDelegate<TenantContext?>(Restore, parent);
    }

    private static void Restore(TenantContext? parent)
    {
        CurrentTenantContext.Value = parent;
    }
}