using System;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.DependencyInjection;

[AttributeUsage(AttributeTargets.Class)]
public class DependencyAttribute : Attribute
{
    public DependencyAttribute() {}

    public DependencyAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }

    public bool AutoRegister { get; set; } = true;
    public ServiceLifetime? Lifetime { get; set; }
    public RegistrationStrategy Strategy { get; set; } = RegistrationStrategy.Add;
    public object? ServiceKey { get; set; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DependencyAttribute<TService> : Attribute, IDependencyAttribute
{
    public DependencyAttribute() {}

    public DependencyAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }

    public Type ServiceType => typeof(TService);
    public ServiceLifetime? Lifetime { get; set; }
    public RegistrationStrategy Strategy { get; set; } = RegistrationStrategy.Add;
    public object? ServiceKey { get; set; }
}