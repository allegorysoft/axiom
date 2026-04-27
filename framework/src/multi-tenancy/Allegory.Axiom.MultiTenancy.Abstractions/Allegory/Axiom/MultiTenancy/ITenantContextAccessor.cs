using System;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.MultiTenancy;

public interface ITenantContextAccessor : ISingletonService
{
    TenantContext? Current { get; }

    IDisposable Set(TenantContext? context = null);

    ValueTask<IDisposable> ChangeAsync(Guid id);

    ValueTask<IDisposable> ChangeAsync(string name);
}