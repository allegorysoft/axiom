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

    // Reduce disposable object allocation
    public virtual void Set(TenantContext? current = null)
    {
        Activity.Current?.SetTag("tenant.id", current?.Id.ToString());
        CurrentTenantContext.Value = current;
    }

    public virtual IDisposable Change(TenantContext? current = null)
    {
        var parent = Current;
        CurrentTenantContext.Value = current;

        Activity.Current?.AddEvent(new ActivityEvent(
            "Tenant switched",
            tags: new ActivityTagsCollection
            {
                {"tenant.id", current?.Id.ToString()}
            }));

        return new DisposableDelegate<TenantContext?>(Restore, parent);
    }

    private static void Restore(TenantContext? parent)
    {
        CurrentTenantContext.Value = parent;
    }
}