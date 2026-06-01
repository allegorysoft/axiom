using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.Interception;

public static class ServiceCollectionExtensions
{
    internal static readonly ConditionalWeakTable<IServiceCollection, ExtraProperties> CollectionProperties = new();

    extension(IServiceCollection collection)
    {
        internal IReadOnlyList<InterceptorDescriptor> GetInterceptors() => CollectionProperties.GetOrCreateValue(collection).Interceptors; 

        public void AddInterceptor<T>(Func<Type, bool> predicate) where T : class, IInterceptor
        {
            CollectionProperties.GetOrCreateValue(collection)
                .Interceptors
                .Add(new InterceptorDescriptor(typeof(T), predicate));
        }

        public void AddInterceptor(Type interceptor, Func<Type, bool> predicate)
        {
            CollectionProperties.GetOrCreateValue(collection)
                .Interceptors
                .Add(new InterceptorDescriptor(interceptor, predicate));
        }
    }

    internal class ExtraProperties
    {
        public List<InterceptorDescriptor> Interceptors { get; } = [];
    }
}