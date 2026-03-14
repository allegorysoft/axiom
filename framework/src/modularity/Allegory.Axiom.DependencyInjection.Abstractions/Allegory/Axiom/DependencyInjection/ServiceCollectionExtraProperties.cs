using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.DependencyInjection;

internal class ServiceCollectionExtraProperties
{
    public List<Action<IServiceCollection>> PostConfigureActions { get; } = [];
}