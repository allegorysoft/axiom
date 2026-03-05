using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Allegory.Axiom.DependencyInjection;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using IProxyGenerator=Allegory.Axiom.DependencyInjection.Proxy.IProxyGenerator;

namespace Allegory.Axiom.Castle;

[Dependency<IProxyGenerator>]
public class ProxyGeneratorCastleAdapter(IServiceProvider serviceProvider) : IProxyGenerator
{
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;
    protected ProxyGenerator Generator { get; } = new();
    protected ConcurrentDictionary<Type, IInterceptor[]> ServiceInterceptorCache { get; } = new();
    protected ConcurrentDictionary<Type, IInterceptor> InterceptorCache { get; } = new();

    public virtual object Create(
        object target,
        Type serviceType,
        IEnumerable<Type> interceptorTypes)
    {
        var interceptors = GetServiceInterceptors(serviceType, interceptorTypes);

        return serviceType.IsInterface
            ? Generator.CreateInterfaceProxyWithTarget(serviceType, target, interceptors)
            : Generator.CreateClassProxyWithTarget(serviceType, target, interceptors);
    }

    protected virtual IInterceptor[] GetServiceInterceptors(Type serviceType, IEnumerable<Type> interceptors)
    {
        return ServiceInterceptorCache.GetOrAdd(
            serviceType,
            t => interceptors
                .Select(ResolveInterceptor)
                .ToArray());
    }

    protected virtual IInterceptor ResolveInterceptor(Type interceptorType)
    {
        return InterceptorCache.GetOrAdd(interceptorType, type =>
        {
            var adapterType = typeof(AxiomInterceptorCastleDeterminationAdapter<>).MakeGenericType(type);
            return (IInterceptor) ServiceProvider.GetRequiredService(adapterType);
        });
    }
}