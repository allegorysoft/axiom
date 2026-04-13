using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Allegory.Axiom.DependencyInjection;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.Interception;

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

        return Generator.CreateInterfaceProxyWithTarget(serviceType, target, interceptors);
    }

    protected virtual Castle.DynamicProxy.IInterceptor[] GetInterceptors(
        IServiceProvider serviceProvider,
        IReadOnlyList<Type> interceptorTypes)
    {
        var interceptors = new Castle.DynamicProxy.IInterceptor[interceptorTypes.Count];

        for (var i = 0; i < interceptors.Length; i++)
        {
            var interceptorType = InterceptorMapCache.GetOrAdd(
                interceptorTypes[i],
                type => typeof(DeterminationInterceptorCastleAdapter<>).MakeGenericType(type));
            interceptors[i] = (Castle.DynamicProxy.IInterceptor) serviceProvider.GetRequiredService(interceptorType);
        }

        return interceptors;
    }
}