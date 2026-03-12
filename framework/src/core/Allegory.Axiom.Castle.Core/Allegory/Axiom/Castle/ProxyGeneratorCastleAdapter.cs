using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Allegory.Axiom.DependencyInjection;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using IProxyGenerator=Allegory.Axiom.DependencyInjection.Proxy.IProxyGenerator;

namespace Allegory.Axiom.Castle;

[Dependency<IProxyGenerator>]
public class ProxyGeneratorCastleAdapter : IProxyGenerator
{
    protected ProxyGenerator Generator { get; } = new();
    protected ConcurrentDictionary<Type, Type> InterceptorMapCache { get; } = new();

    public virtual object Create(
        IServiceProvider serviceProvider,
        object target,
        Type serviceType,
        IReadOnlyList<Type> interceptorTypes)
    {
        var interceptors = GetInterceptors(serviceProvider, interceptorTypes);

        return serviceType.IsInterface
            ? Generator.CreateInterfaceProxyWithTarget(serviceType, target, interceptors)
            : Generator.CreateClassProxyWithTarget(serviceType, target, interceptors);
    }

    protected virtual IInterceptor[] GetInterceptors(
        IServiceProvider serviceProvider,
        IReadOnlyList<Type> interceptorTypes)
    {
        var interceptors = new IInterceptor[interceptorTypes.Count];

        for (var i = 0; i < interceptors.Length; i++)
        {
            var interceptorType = InterceptorMapCache.GetOrAdd(
                interceptorTypes[i],
                type => typeof(AxiomInterceptorCastleDeterminationAdapter<>).MakeGenericType(type));
            interceptors[i] = (IInterceptor) serviceProvider.GetRequiredService(interceptorType);
        }

        return interceptors;
    }
}