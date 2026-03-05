using System;
using System.Collections.Generic;

namespace Allegory.Axiom.DependencyInjection.Proxy;

public interface IProxyGenerator : ISingletonService
{
    object Create(object target, Type serviceType, IEnumerable<Type> interceptors);
}