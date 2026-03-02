using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.DependencyInjection.Proxy;

internal sealed class ServiceInterceptorBinder
{
    public IServiceCollection Collection { get; }
    public List<InterceptorDescriptor> Interceptors { get; }

    public static void Apply(IServiceCollection collection, List<InterceptorDescriptor> interceptors)
        => new ServiceInterceptorBinder(collection, interceptors).ApplyInterceptors();

    private ServiceInterceptorBinder(
        IServiceCollection collection,
        List<InterceptorDescriptor> interceptors)
    {
        Collection = collection;
        Interceptors = interceptors;
    }

    public void ApplyInterceptors()
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
                    //Currently `ImplementationInstance`, `ImplementationFactory` not supported.
                    var implementationType = t.IsKeyedService ? t.KeyedImplementationType : t.ImplementationType;
                    return implementationType != null && interceptor.Predicate(implementationType);
                }
            );

            foreach (var service in services)
            {
                serviceInterceptors.TryAdd(service, []);
                serviceInterceptors[service].Add(interceptor.Interceptor);
            }
        }

        return serviceInterceptors;
    }

    private void RegisterService(ServiceDescriptor service, List<Type> interceptors)
    {
        var newService = service.IsKeyedService ?
            GetKeyedService(service, interceptors) :
            GetService(service, interceptors);

        Collection.Remove(service);
        Collection.Add(newService);
    }

    private static ServiceDescriptor GetService(ServiceDescriptor service, List<Type> interceptors)
    {
        return ServiceDescriptor.Describe(
            service.ServiceType,
            provider =>
            {
                var implementation = ActivatorUtilities.CreateInstance(provider, service.ImplementationType!);
                var proxy = provider.GetRequiredService<IProxyGenerator>();
                return proxy.Create(implementation, service.ServiceType, interceptors, provider);
            },
            service.Lifetime
        );
    }

    private static ServiceDescriptor GetKeyedService(ServiceDescriptor service, List<Type> interceptors)
    {
        return ServiceDescriptor.DescribeKeyed(
            service.ServiceType,
            service.ServiceKey,
            (provider, _) =>
            {
                var implementation = ActivatorUtilities.CreateInstance(provider, service.KeyedImplementationType!);
                var proxy = provider.GetRequiredService<IProxyGenerator>();
                return proxy.Create(implementation, service.ServiceType, interceptors, provider);
            },
            service.Lifetime
        );
    }
}