using System;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.MultiTenancy;

public interface ITenantContextAccessor : ISingletonService
{
    TenantContext? Current { get; }
    void Set(TenantContext? context = null);
    IDisposable Change(TenantContext? current = null);
}