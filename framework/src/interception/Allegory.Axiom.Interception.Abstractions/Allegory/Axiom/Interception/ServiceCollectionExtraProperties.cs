using System.Collections.Generic;

namespace Allegory.Axiom.Interception;

internal class ServiceCollectionExtraProperties
{
    public List<InterceptorDescriptor> Interceptors { get; } = [];
}