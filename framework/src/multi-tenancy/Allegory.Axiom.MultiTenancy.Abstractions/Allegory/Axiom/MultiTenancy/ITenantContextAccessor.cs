using System;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Disposables;

namespace Allegory.Axiom.MultiTenancy;

public interface ITenantContextAccessor : ISingletonService
{
    TenantContext? Current { get; }
    void Set(TenantContext? context = null);
    IDisposable Change(TenantContext? current = null);
}