using System;
using System.Collections.Generic;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.Interception;

public interface IProxyGenerator : ISingletonService
{
    object Create(IServiceProvider serviceProvider, object target, Type serviceType, IReadOnlyList<Type> interceptors);
}