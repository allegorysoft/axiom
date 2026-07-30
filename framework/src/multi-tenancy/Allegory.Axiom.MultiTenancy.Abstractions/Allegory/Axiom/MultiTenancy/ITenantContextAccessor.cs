using System;

namespace Allegory.Axiom.MultiTenancy;

public interface ITenantContextAccessor
{
    TenantContext? Current { get; }
    TenantContext RequiredCurrent { get; }
    void Set(TenantContext? current = null);
    IDisposable Change(TenantContext? current = null);
}