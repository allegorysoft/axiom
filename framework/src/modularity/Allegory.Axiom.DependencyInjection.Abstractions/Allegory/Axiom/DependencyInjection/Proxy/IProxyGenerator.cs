using System;
using System.Collections.Generic;

namespace Allegory.Axiom.DependencyInjection.Proxy;

public interface IProxyGenerator : ISingletonService
{
    object Create(IServiceProvider serviceProvider, object target, Type serviceType, IReadOnlyList<Type> interceptors);
}