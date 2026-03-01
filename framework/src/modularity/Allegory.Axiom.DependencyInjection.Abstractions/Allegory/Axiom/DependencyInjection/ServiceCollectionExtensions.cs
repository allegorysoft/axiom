using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Allegory.Axiom.DependencyInjection.Proxy;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private static readonly ConditionalWeakTable<
        IServiceCollection,
        ServiceCollectionExtraProperties> ExtraProperties = new();

    extension(IServiceCollection collection)
    {
        public IReadOnlyList<Action<IServiceCollection>> PostConfigureActions =>
            ExtraProperties.GetOrCreateValue(collection).PostConfigureActions;

        internal IReadOnlyList<InterceptorDescriptor> Interceptors =>
            ExtraProperties.GetOrCreateValue(collection).Interceptors;

        public void AddPostConfigureAction(Action<IServiceCollection> action)
        {
            ExtraProperties.GetOrCreateValue(collection)
                .PostConfigureActions
                .Add(action);
        }

        public void RegisterInterceptor<T>(Func<Type, bool> predicate) where T : IAxiomInterceptor, new()
        {
            ExtraProperties.GetOrCreateValue(collection)
                .Interceptors
                .Add(new InterceptorDescriptor(typeof(T), predicate));
        }

        public void ApplyInterceptors()
        {
            var interceptors = new Dictionary<ServiceDescriptor, List<Type>>();

            foreach (var interceptor in collection.Interceptors)
            {
                var services = collection.Where(t =>
                    {
                        var implementationType = t.IsKeyedService ? t.KeyedImplementationType : t.ImplementationType;
                        return implementationType != null && interceptor.Predicate(implementationType);
                    }
                );

                foreach (var service in services)
                {
                    interceptors.TryAdd(service, []);
                    interceptors[service].Add(interceptor.Interceptor);
                }
            }

            foreach (var interceptor in interceptors)
            {
                var service = interceptor.Key;
                collection.Remove(service);

                if (service.IsKeyedService)
                {
                    var serviceDescriptor = new ServiceDescriptor(
                        service.ServiceType,
                        service.ServiceKey,
                        (sp, key) =>
                        {
                            return null;
                        },
                        service.Lifetime
                    );
                }
                else {}
            }
        }
    }
}