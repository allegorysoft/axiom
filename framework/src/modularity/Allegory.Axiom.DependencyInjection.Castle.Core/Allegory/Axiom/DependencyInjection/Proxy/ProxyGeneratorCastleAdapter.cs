using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.DependencyInjection.Proxy;

[Dependency<IProxyGenerator>]
public class ProxyGeneratorCastleAdapter : IProxyGenerator
{
    //CreateClassProxy, CreateClassProxy<>, CreateClassProxyWithTarget
    //CreateInterfaceProxyWithTarget, CreateInterfaceProxyWithTargetInterface
    //CreateInterfaceProxyWithoutTarget, CreateInterfaceProxyWithoutTarget<> 
    //AsyncInterceptorBase, AsyncDeterminationInterceptor : IInterceptor
    protected ProxyGenerator Generator { get; } = new();
    protected internal ConcurrentDictionary<Type, Type> InterceptorMapCache { get; } = new();

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