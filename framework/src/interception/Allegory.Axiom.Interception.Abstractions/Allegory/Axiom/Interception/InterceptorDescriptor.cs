using System;

namespace Allegory.Axiom.Interception;

internal readonly struct InterceptorDescriptor
{
    public InterceptorDescriptor(Type interceptor, Func<Type, bool> predicate)
    {
        if (!typeof(IAxiomInterceptor).IsAssignableFrom(interceptor))
        {
            throw new ArgumentException(
                $"Type '{interceptor}' must implement {nameof(IAxiomInterceptor)}.",
                nameof(interceptor));
        }

        Predicate = predicate;
        Interceptor = interceptor;
    }

    public Func<Type, bool> Predicate { get; }
    public Type Interceptor { get; }
}