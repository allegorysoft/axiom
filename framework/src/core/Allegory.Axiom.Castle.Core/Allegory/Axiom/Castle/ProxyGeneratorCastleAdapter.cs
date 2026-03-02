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
public class ProxyGeneratorCastleAdapter : IProxyGenerator
{
    private readonly ConcurrentDictionary<Type, IInterceptor> _interceptorCache = new();

    public ProxyGenerator Generator { get; } = new();

    public virtual object Create(object target, Type serviceType, IEnumerable<Type> interceptorTypes, IServiceProvider provider)
    {
        var interceptors = interceptorTypes
            .Select(type => ResolveInterceptor(type, provider))
            .ToArray();

        return serviceType.IsInterface
            ? Generator.CreateInterfaceProxyWithTarget(serviceType, target, interceptors)
            : Generator.CreateClassProxyWithTarget(target, interceptors);
    }

    protected virtual IInterceptor ResolveInterceptor(Type interceptorType, IServiceProvider provider)
    {
        return _interceptorCache.GetOrAdd(interceptorType, type =>
        {
            var adapterType = typeof(AxiomInterceptorCastleDeterminationAdapter<>).MakeGenericType(type);
            return (IInterceptor) provider.GetRequiredService(adapterType);
        });
    }
}