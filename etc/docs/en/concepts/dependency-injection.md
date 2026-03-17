---
title: Dependency Injection
description: Convention-based, attribute-driven automatic service registration for Microsoft.Extensions.DependencyInjection.
---

# Dependency Injection

Axiom dependency injection system provides convention-based assembly scanning built on top of `Microsoft.Extensions.DependencyInjection.Abstractions`, automatically registering your services with no manual `services.Add*()` calls needed. If you need to register services manually, see the [Configure Application](./modularity/overview#configure-application) section. You can add it to your project via NuGet:

```bash
dotnet add package Allegory.Axiom.DependencyInjection.Abstractions
```

The library offers three complementary registration approaches. You can mix them freely within the same assembly:

| Approach | How |
|---|---|
| Marker interfaces | Implement `ITransientService`, `IScopedService`, or `ISingletonService` |
| `[Dependency]` attribute | Declare lifetime, strategy, key, or opt-out on the class |
| `[Dependency<TService>]` attribute | Explicitly map an implementation to one or more service types |

Assembly scanning is driven by `AssemblyDependencyRegistrar`, which discovers all eligible types and registers them into `IServiceCollection`.
```csharp
var registrar = new AssemblyDependencyRegistrar(services);
registrar.Register(typeof(Program).Assembly);
```

## Marker Interfaces

The simplest way to declare a service lifetime. No attributes required, just implement the interface.

| Interface | Lifetime |
|---|---|
| `ITransientService` | A new instance is created for every resolution request. |
| `IScopedService` | One instance per DI scope (e.g. per HTTP request in ASP.NET Core). |
| `ISingletonService` | One instance for the lifetime of the application. |
```csharp
public class OrderService : IOrderService, ITransientService { }
public class UserSession  : IUserSession,  IScopedService    { }
public class AppConfig    : IAppConfig,    ISingletonService { }
```

### Interface-to-implementation name matching

When the scanner finds a type, it automatically registers matching interfaces based on naming convention. The interface name with the leading `I` stripped must match the end of the class name (case-insensitive).
```csharp
// ✅ IOrderService → OrderService         — matched, registered
// ✅ IOrderService → ExtendedOrderService — matched, registered
// ❌ IOrderManager → OrderService         — not matched, skipped
```

### Lifetime inheritance

Marker interfaces are inherited. A subclass of a `ISingletonService` base class is itself treated as a singleton unless overridden by `[Dependency]`.
```csharp
public class Base    : ISingletonService { }
public class Derived : Base { }
// Derived → registered as Singleton
```

## `[Dependency]` Attribute

Use this attribute when you need explicit control over lifetime, registration strategy, service key, or want to opt out of auto-registration entirely. It participates in the same interface-name matching logic as marker interfaces.

| Property | Type | Default | Description |
|---|---|---|---|
| `AutoRegister` | `bool` | `true` | Set to `false` to skip this class entirely. |
| `Lifetime` | `ServiceLifetime?` | `null` | Overrides the lifetime from marker interfaces. Falls back to the marker interface lifetime when `null`. |
| `Strategy` | `RegistrationStrategy` | `Add` | Controls how the descriptor enters the container. |
| `ServiceKey` | `object?` | `null` | Registers as a keyed service when set. |
```csharp
// Basic lifetime
[Dependency(ServiceLifetime.Transient)]
public class ReportGenerator : IReportGenerator { }

// Opt out
[Dependency(AutoRegister = false)]
public class ManuallyWiredService : IScopedService { }

// Keyed service
[Dependency(ServiceKey = "primary")]
public class PrimaryCache : ICache, ITransientService { }

// Override marker interface lifetime, attribute wins
[Dependency(ServiceLifetime.Transient)]
public class OverriddenService : ISingletonService { }
```

## `[Dependency<TService>]` Attribute

A generic, multi-apply variant that explicitly maps an implementation to one or more specific service types. When this attribute is present, **interface-name matching is bypassed entirely**, only the listed `TService` types are registered as service types.

| Property | Type | Default | Description |
|---|---|---|---|
| `ServiceType` | `Type` | `typeof(TService)` | The service type this registration maps to. Read-only. |
| `Lifetime` | `ServiceLifetime?` | `null` | Lifetime for this specific registration. Falls back to `[Dependency]` or marker interfaces. |
| `Strategy` | `RegistrationStrategy` | `Add` | Registration strategy for this specific service type. |
| `ServiceKey` | `object?` | `null` | Keyed service key for this specific registration. |
```csharp
// Single explicit mapping
[Dependency<IPaymentGateway>(ServiceLifetime.Transient)]
public class StripeGateway : IPaymentGateway { }

// Multiple mappings, independent lifetimes per service type
[Dependency<IZooManager>(ServiceLifetime.Transient)]
[Dependency<IHooManager>(ServiceLifetime.Scoped)]
public class AnimalManager : IZooManager, IHooManager, ISingletonService { }
// Registers:
//   IZooManager   → AnimalManager (Transient)
//   IHooManager   → AnimalManager (Scoped)
//   AnimalManager → AnimalManager (Singleton, from ISingletonService)

// No default lifetime — only the explicit service type is registered, not the class itself
[Dependency<IGooManager>(ServiceLifetime.Scoped)]
public class GooManager : IGooManager { }

// Keyed service
[Dependency<IGenericKeyedService>(ServiceKey = 1)]
public class KeyedManager : IGenericKeyedService, ITransientService { }
```

## `RegistrationStrategy`

Controls how a `ServiceDescriptor` is added to `IServiceCollection`.

| Value | Behavior |
|---|---|
| `Add` | Always adds. Multiple registrations for the same service type are allowed. |
| `TryAdd` | Adds only if no registration for this service type exists yet. |
| `Replace` | Removes the existing registration and adds the new one. |
```csharp
// Added normally
public class CustomerManager : ICustomerManager, ITransientService { }

// Replaces CustomerManager as the implementation for ICustomerManager
[Dependency(Strategy = RegistrationStrategy.Replace)]
public class ReplacedCustomerManager : ICustomerManager, ITransientService { }

// Ignored, ICustomerManager is already registered
[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class FallbackCustomerManager : ICustomerManager, ITransientService { }
```

## Lifetime Resolution

The lifetime of a registration is resolved in the following order of precedence:

1. `[Dependency<TService>].Lifetime` if specified on the generic attribute for that service
2. `[Dependency].Lifetime` if specified on the default attribute
3. Marker interface: `ITransientService`, `IScopedService`, or `ISingletonService`

The behavior differs depending on whether the lifetime is resolved for an explicit service registration (`[Dependency<TService>]`) or a default self/interface registration:

- For **explicit service registrations** (`[Dependency<TService>]`), if no lifetime can be resolved, an `InvalidOperationException` is thrown at scan time.
- For **default registration** (self + name-matched interfaces), if no lifetime can be resolved, the class is silently skipped. To be eligible, a class must either implement a marker interface or specify `Lifetime` on a `[Dependency]` attribute.
```csharp
// ✅ Lifetime from marker interface
public class OrderService : IOrderService, ITransientService { }

// ✅ Lifetime from [Dependency] attribute
[Dependency(ServiceLifetime.Scoped)]
public class ReportService : IReportService { }

// ⏭️ Silently skipped, no lifetime resolvable, no interfaces to match
[Dependency]
public class AmbiguousService { }

// 💥 InvalidOperationException, IAmbiguousService matches name convention but no lifetime resolvable
[Dependency]
public class AmbiguousService : IAmbiguousService { }

// ✅ IGooManager registered as Scoped, GooManager itself is skipped (no default lifetime)
[Dependency<IGooManager>(ServiceLifetime.Scoped)]
public class GooManager : IGooManager { }

// 💥 InvalidOperationException, explicit [Dependency<TService>] with no lifetime
[Dependency<IFooManager>]
public class BadManager : IFooManager { }
```

## Generic Services

Both marker interfaces and attributes work with open and closed generic types.
```csharp
// Open generic, registered as IOrderRepository<>
public class OrderRepository<T> : IOrderRepository<T>, ITransientService { }

// Closed generic, registered as IOrderRepository<int>
public class IntOrderRepository : IOrderRepository<int>, ITransientService { }
```

::: warning
When registering an open generic, the generic arguments of the service type and the implementation must match exactly, otherwise an `InvalidOperationException` is thrown at scan time.
:::

## `AssemblyDependencyRegistrar`

The entry point for assembly scanning. Extend this class to customize discovery or registration behaviour.
```csharp
public class AssemblyDependencyRegistrar(IServiceCollection serviceCollection)
{
    protected internal IServiceCollection ServiceCollection { get; }

    public virtual void Register(Assembly assembly);
    protected virtual IEnumerable<Type> GetImplementationTypes(Assembly assembly);
    protected virtual void RegisterImplementation(Type implementation);
    protected virtual void RegisterService(ServiceDescriptor descriptor, RegistrationStrategy strategy);
}
```

| Method | Description |
|---|---|
| `Register(Assembly)` | Scans the assembly and registers all eligible types. |
| `GetImplementationTypes(Assembly)` | Returns types eligible for registration. Override to change the discovery filter. |
| `RegisterImplementation(Type)` | Processes a single type. Override to add custom pre/post logic. |
| `RegisterService(ServiceDescriptor, RegistrationStrategy)` | Applies a single descriptor to the container. Override to intercept all registrations. |

A type is eligible for scanning if it is a non-abstract class **and** at least one of the following is true:

- Implements `ITransientService`, `IScopedService`, or `ISingletonService`
- Has `[Dependency]` applied (including inherited)
- Has `[Dependency<TService>]` applied (including inherited)
```csharp
public class MyRegistrar(IServiceCollection services) : AssemblyDependencyRegistrar(services)
{
    protected override IEnumerable<Type> GetImplementationTypes(Assembly assembly)
    {
        return base.GetImplementationTypes(assembly)
            .Where(t => !t.Namespace!.EndsWith(".Internal"));
    }

    protected override void RegisterService(ServiceDescriptor descriptor, RegistrationStrategy strategy)
    {
        Console.WriteLine($"Registering {descriptor.ServiceType.Name} [{descriptor.Lifetime}]");
        base.RegisterService(descriptor, strategy);
    }
}
```

## Post-Configure Actions

`ServiceCollectionExtensions` adds two extension methods to `IServiceCollection` for deferring configuration callbacks until after all registrations are complete.
```csharp
services.AddPostConfigureAction(sc =>
{
    sc.AddSingleton<IStartupValidator, StartupValidator>();
});

// Execute all queued actions in registration order
services.ExecutePostConfigureActions();
```

Each `IServiceCollection` instance maintains its own independent action queue, state is never shared between collections. Actions are stored in a `ConditionalWeakTable` and do not prevent the collection from being garbage collected.
