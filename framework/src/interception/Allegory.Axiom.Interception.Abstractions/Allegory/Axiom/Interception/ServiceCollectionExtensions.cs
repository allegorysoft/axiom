using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.Interception;

public static class ServiceCollectionExtensions
{
    internal static readonly ConditionalWeakTable<
        IServiceCollection,
        ServiceCollectionExtraProperties> ExtraProperties = new();

    extension(IServiceCollection collection)
    {
        internal IReadOnlyList<InterceptorDescriptor> Interceptors =>
            ExtraProperties.GetOrCreateValue(collection).Interceptors;

        public void AddInterceptor<T>(Func<Type, bool> predicate) where T : class, IInterceptor
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