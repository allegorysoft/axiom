using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Allegory.Axiom.DependencyInjection;

public class AssemblyDependencyRegistrar(IServiceCollection serviceCollection)
{
    protected internal IServiceCollection ServiceCollection { get; } = serviceCollection;

    public virtual void Register(Assembly assembly)
    {
        var implementations = GetImplementationTypes(assembly);

        foreach (var implementation in implementations)
        {
            RegisterImplementation(implementation);
        }
    }

    protected virtual IEnumerable<Type> GetImplementationTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t is {IsClass: true, IsAbstract: false})
            .Where(t => typeof(ITransientService).IsAssignableFrom(t) ||
                        typeof(IScopedService).IsAssignableFrom(t) ||
                        typeof(ISingletonService).IsAssignableFrom(t) ||
                        t.IsDefined(typeof(DependencyAttribute), inherit: true) ||
                        t.IsDefined(typeof(DependencyAttribute<>), inherit: true));
    }

    protected virtual void RegisterImplementation(Type implementation)
    {
        var implementationType = new ImplementationType(implementation);
        if (implementationType.Attribute?.AutoRegister == false)
        {
            return;
        }

        var defaultLifetime = implementationType.TryGetLifetime();
        if (defaultLifetime.HasValue)
        {
            RegisterService(
                new ServiceDescriptor(
                    implementationType.Type,
                    implementationType.Attribute?.ServiceKey,
                    implementationType.Type,
                    defaultLifetime.Value),
                implementationType.Attribute?.Strategy ?? RegistrationStrategy.Add);
        }

        RegisterImplementationServices(implementationType);
    }

    private void RegisterImplementationServices(ImplementationType implementation)
    {
        if (implementation.ServiceAttributes.Count > 0)
        {
            foreach (var service in implementation.ServiceAttributes)
            {
                RegisterService(
                    new ServiceDescriptor(
                        implementation.GetServiceType(service.ServiceType),
                        service.ServiceKey,
                        implementation.Type,
                        implementation.GetLifetime(service)),
                    service.Strategy);
            }
        }
        else
        {
            foreach (var serviceType in implementation.Interfaces.Where(implementation.ShouldRegister))
            {
                RegisterService(
                    new ServiceDescriptor(
                        implementation.GetServiceType(serviceType),
                        implementation.Attribute?.ServiceKey,
                        implementation.Type,
                        implementation.GetLifetime()),
                    implementation.Attribute?.Strategy ?? RegistrationStrategy.Add);
            }
        }
    }

    protected virtual void RegisterService(
        ServiceDescriptor serviceDescriptor,
        RegistrationStrategy strategy)
    {
        switch (strategy)
        {
            case RegistrationStrategy.Add:
                ServiceCollection.Add(serviceDescriptor);
                break;
            case RegistrationStrategy.TryAdd:
                ServiceCollection.TryAdd(serviceDescriptor);
                break;
            case RegistrationStrategy.Replace:
                ServiceCollection.Replace(serviceDescriptor);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
        }
    }
}