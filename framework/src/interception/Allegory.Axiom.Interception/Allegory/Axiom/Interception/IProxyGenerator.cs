using System;
using System.Collections.Generic;

namespace Allegory.Axiom.Interception;

public interface IProxyGenerator
{
    object Create(IServiceProvider serviceProvider, object target, Type serviceType, IReadOnlyList<Type> interceptors);
}