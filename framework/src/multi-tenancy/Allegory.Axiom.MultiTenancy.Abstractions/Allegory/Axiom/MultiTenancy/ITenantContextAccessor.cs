using System;

namespace Allegory.Axiom.MultiTenancy;

public interface ITenantContextAccessor
{
    static Func<TenantContext?> TryGetCurrent { get; set; } = null!;

    TenantContext? Current { get; }
    TenantContext RequiredCurrent { get; }
    void Set(TenantContext? current = null);
    IDisposable Change(TenantContext? current = null);
}