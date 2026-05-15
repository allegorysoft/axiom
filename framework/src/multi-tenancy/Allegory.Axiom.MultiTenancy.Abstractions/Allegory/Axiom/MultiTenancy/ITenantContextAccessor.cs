using System;

namespace Allegory.Axiom.MultiTenancy;

public interface ITenantContextAccessor
{
    TenantContext? Current { get; }
    void Set(TenantContext? current = null);
    IDisposable Change(TenantContext? current = null);
}