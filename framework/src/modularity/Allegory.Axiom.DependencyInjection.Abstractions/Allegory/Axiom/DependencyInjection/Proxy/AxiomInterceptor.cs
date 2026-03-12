using System;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.DependencyInjection.Proxy;

public abstract class AxiomInterceptor : IAxiomInterceptor
{
    internal static readonly AsyncLocal<IServiceProvider> CurrentServiceProvider = new();

    protected IServiceProvider ServiceProvider => CurrentServiceProvider.Value!;

    public abstract Task InterceptAsync(IAxiomInterceptorContext context);
}