using System;
using System.Collections.Generic;
using Allegory.Axiom.DependencyInjection.Proxy;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.DependencyInjection;

internal class ServiceCollectionExtraProperties
{
    public List<Action<IServiceCollection>> PostConfigureActions { get; } = [];
    public List<InterceptorDescriptor> Interceptors { get; } = [];

    public ServiceCollectionExtraProperties() {}
}