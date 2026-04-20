using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.Interception;

internal sealed class ServiceInterceptorBinder(IServiceCollection collection)
{
    public IServiceCollection Collection { get; } = collection;
    public IReadOnlyList<InterceptorDescriptor> Interceptors { get; } = collection.Interceptors;

    public static void Apply(IServiceCollection collection)
        => new ServiceInterceptorBinder(collection).ApplyInterceptors();

    private void ApplyInterceptors()
    {
        foreach (var serviceInterceptor in GetServiceInterceptors())
        {
            RegisterService(serviceInterceptor.Key, serviceInterceptor.Value);
        }
    }

    private Dictionary<ServiceDescriptor, List<Type>> GetServiceInterceptors()
    {
        var serviceInterceptors = new Dictionary<ServiceDescriptor, List<Type>>();

        foreach (var interceptor in Interceptors)
        {
            var services = Collection.Where(t =>
                {
                    var implementationType = t.IsKeyedService ? t.KeyedImplementationType : t.ImplementationType;
                    return t.ServiceType.IsInterface && implementationType != null && interceptor.Predicate(implementationType);
                }
            );

            foreach (var service in services)
            {
                if (!serviceInterceptors.TryGetValue(service, out var list))
                {
                    serviceInterceptors[service] = list = [];
                }

                list.Add(interceptor.Interceptor);
            }
        }

        return serviceInterceptors;
    }

    private void RegisterService(ServiceDescriptor service, List<Type> interceptors)
    {
        var proxyService = service.IsKeyedService
            ? GetKeyedService(service, interceptors)
            : GetService(service, interceptors);

        Collection.Remove(service);
        Collection.Add(proxyService);
    }

    private static ServiceDescriptor GetService(ServiceDescriptor service, List<Type> interceptors)
    {
        var factory = ActivatorUtilities.CreateFactory(service.ImplementationType!, Type.EmptyTypes);

        return ServiceDescriptor.Describe(
            service.ServiceType,
            provider =>
            {
                var implementation = factory(provider, null);
                var proxy = provider.GetRequiredService<IProxyGenerator>();
                return proxy.Create(provider, implementation, service.ServiceType, interceptors);
            },
            service.Lifetime
        );
    }

    private static ServiceDescriptor GetKeyedService(ServiceDescriptor service, List<Type> interceptors)
    {
        var factory = ActivatorUtilities.CreateFactory(service.KeyedImplementationType!, Type.EmptyTypes);

        return ServiceDescriptor.DescribeKeyed(
            service.ServiceType,
            service.ServiceKey,
            (provider, _) =>
            {
                var implementation = factory(provider, null);
                var proxy = provider.GetRequiredService<IProxyGenerator>();
                return proxy.Create(provider, implementation, service.ServiceType, interceptors);
            },
            service.Lifetime
        );
    }
}