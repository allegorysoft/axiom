using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Allegory.Axiom.DependencyInjection;

public class AssemblyDependencyRegistrar(IServiceCollection serviceCollection)
{
    public static HashSet<Type> IgnoredServiceTypes { get; } = [];

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

        var services = implementationType.GetServices().ToList();

        foreach (var (descriptor, strategy) in services)
        {
            RegisterService(descriptor, strategy);
        }

        if (services.Count == 0 || implementationType.Attribute?.SelfRegister == true)
        {
            RegisterService(
                new ServiceDescriptor(
                    implementationType.Type,
                    implementationType.Attribute?.ServiceKey,
                    implementationType.Type,
                    implementationType.GetLifetime()),
                implementationType.Attribute?.Strategy ?? RegistrationStrategy.Add);
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