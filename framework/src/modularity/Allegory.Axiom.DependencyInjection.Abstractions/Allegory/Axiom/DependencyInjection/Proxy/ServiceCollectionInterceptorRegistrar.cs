using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.DependencyInjection.Proxy;

internal class ServiceCollectionInterceptorRegistrar(
    IServiceCollection collection,
    List<InterceptorDescriptor> interceptors)
{
    public IServiceCollection Collection { get; } = collection;
    public List<InterceptorDescriptor> Interceptors { get; } = interceptors;

    public void ApplyInterceptors()
    {
        var serviceInterceptors = GetServiceInterceptors();

        foreach (var serviceInterceptor in GetServiceInterceptors())
        {
            Collection.Remove(serviceInterceptor.Key);
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
        Collection.Add(newService);
    }

    private ServiceDescriptor GetService(ServiceDescriptor service, List<Type> interceptors)
    {
        return null;
    }

    private ServiceDescriptor GetKeyedService(ServiceDescriptor service, List<Type> interceptors)
    {
        //Collection. service.KeyedImplementationType!
        return null;
    }
}