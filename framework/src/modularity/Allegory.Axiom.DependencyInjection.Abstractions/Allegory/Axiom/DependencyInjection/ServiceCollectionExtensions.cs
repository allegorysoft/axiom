using System;
using System.Runtime.CompilerServices;
using Allegory.Axiom.DependencyInjection.Proxy;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.DependencyInjection;

public static class ServiceCollectionExtensions
{
    internal static readonly ConditionalWeakTable<
        IServiceCollection,
        ServiceCollectionExtraProperties> ExtraProperties = new();

    extension(IServiceCollection collection)
    {
        public void AddPostConfigureAction(Action<IServiceCollection> action)
        {
            ExtraProperties.GetOrCreateValue(collection)
                .PostConfigureActions
                .Add(action);
        }

        public void ExecutePostConfigureActions()
        {
            var extraProperties = ExtraProperties.GetOrCreateValue(collection);

            foreach (var action in extraProperties.PostConfigureActions)
            {
                action(collection);
            }

            ServiceInterceptorBinder.Apply(collection, extraProperties.Interceptors);
        }

        public void AddInterceptor<T>(Func<Type, bool> predicate) where T : IAxiomInterceptor, new()
        {
            ExtraProperties.GetOrCreateValue(collection)
                .Interceptors
                .Add(new InterceptorDescriptor(typeof(T), predicate));
        }

        public void AddInterceptor(Type interceptor, Func<Type, bool> predicate)
        {
            ExtraProperties.GetOrCreateValue(collection)
                .Interceptors
                .Add(new InterceptorDescriptor(interceptor, predicate));
        }
    }
}