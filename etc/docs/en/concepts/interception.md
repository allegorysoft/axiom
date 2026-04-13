---
title: Interception
description: AOP-style method interception via Castle DynamicProxy for cross-cutting concerns.
---

# Interception

Axiom interception lets you transparently wrap service methods with cross-cutting logic such as logging, caching, authorization, or transaction management without touching your business code. It works by replacing registered services with dynamic proxies at container build time.

Two packages are involved:

```bash
dotnet add package Allegory.Axiom.Interception.Abstractions
dotnet add package Allegory.Axiom.Interception.Castle.Core
```

`Allegory.Axiom.Interception.Abstractions` provides the core interfaces and registration API. `Allegory.Axiom.Interception.Castle.Core` provides the [Castle DynamicProxy](https://www.castleproject.org/projects/dynamicproxy/) implementation that creates the actual proxies.

## Writing an Interceptor

Implement `IInterceptor` with a single `InterceptAsync` method. Call `context.ProceedAsync()` to invoke the next interceptor in the chain or the actual method.

```csharp
using Allegory.Axiom.Interception;

namespace MyApp.Interceptors;

public class LoggingInterceptor : IInterceptor, ISingletonService
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public async Task InterceptAsync(IInterceptorContext context)
    {
        _logger.LogInformation("Calling {Method}", context.Method.Name);
        await context.ProceedAsync();
        _logger.LogInformation("Finished {Method}", context.Method.Name);
    }
}
```

Interceptors are resolved from the DI container, so constructor injection works normally. Register them using a [DI marker](./dependency-injection.md#marker-interfaces) interface or `[Dependency]` attribute just like any other service.

## IInterceptorContext

The context passed to `InterceptAsync` exposes everything about the current invocation.

| Member | Description |
|---|---|
| `Method` | The `MethodInfo` of the method being called. |
| `Target` | The actual implementation instance being proxied. |
| `Arguments` | The arguments passed to the method. |
| `ReturnValue` | The return value. Readable and writable for methods with return types. |
| `ProceedAsync()` | Invokes the next interceptor or the actual method. |

You can modify arguments before calling `ProceedAsync()`, or modify the return value after.

```csharp
public async Task InterceptAsync(IInterceptorContext context)
{
    // Modify an argument before the call
    context.Arguments[0] = ((string) context.Arguments[0]!).Trim();

    await context.ProceedAsync();

    // Modify the return value after the call
    if (context.ReturnValue is string result)
        context.ReturnValue = result.ToUpper();
}
```

## Registering Interceptors

Register interceptors via `services.AddInterceptor<T>(predicate)` in your [application packages](./modularity/overview.md#application-packages). The predicate receives the implementation type of each registered service. The service type must be an interface, otherwise it is **skipped**.

```csharp
internal sealed class MyAppPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        // Intercept all services that implement IOrderService
        builder.Services.AddInterceptor<LoggingInterceptor>(
            t => typeof(IOrderService).IsAssignableFrom(t));

        // Intercept a specific type
        // ✓ services.AddTransient<IProductRepository, ProductRepository>() - intercepted
        // ✗ services.AddTransient<ProductRepository>() - skipped
        builder.Services.AddInterceptor<CachingInterceptor>(
            t => t == typeof(ProductRepository));

        return ValueTask.CompletedTask;
    }
}
```

`AddInterceptor` does not apply proxies immediately. It queues the registration. At the end of package configurations, Axiom runs `ServiceInterceptorBinder.Apply` as a post-configure action, which replaces all matched service descriptors with proxy factories.

## Multiple Interceptors

You can register multiple interceptors for the same service. They are invoked in registration order, forming a pipeline. Each interceptor calls `context.ProceedAsync()` to pass control to the next one.

```csharp
builder.Services.AddInterceptor<LoggingInterceptor>(t => typeof(IOrderService).IsAssignableFrom(t));
builder.Services.AddInterceptor<CachingInterceptor>(t => typeof(IOrderService).IsAssignableFrom(t));

// Call order: LoggingInterceptor → CachingInterceptor → actual method
```

## Interceptor Lifetime

Interceptors support all three lifetimes. Register them using the appropriate marker interface.

```csharp
public class SingletonInterceptor : IInterceptor, ISingletonService { ... }
public class ScopedInterceptor    : IInterceptor, IScopedService    { ... }
public class TransientInterceptor : IInterceptor, ITransientService { ... }
```

Interceptors act as wrappers around your services. For stability, their lifetimes must align with the services they proxy and the dependencies they consume.

### Key Risks
* **Captive Dependencies:** A long-lived Interceptor (Singleton) holding a short-lived service (Scoped). The Scoped service never disposes, causing memory leaks or stale state.
* **Object Disposed Errors:** A long-lived service using a short-lived Interceptor. The Interceptor may be disposed of while the service is still active.

### Best Practices
* **Match Lifetimes:** Ensure a dependency lives at least as long as its consumer.
* **Default to Singletons:** Since most interceptors are stateless logic, making them **Singletons** is the safest way to avoid lifecycle conflicts and maximize performance.

::: tip Rule of Thumb
Keep lifetimes aligned. When in doubt, use a Singleton interceptor and avoid injecting Transient/Scoped services into it.
:::

## Keyed Services

Interception works with keyed services. The proxy preserves the service key.

```csharp
builder.Services.AddKeyedScoped<IOrderService, OrderService>("primary");
builder.Services.AddInterceptor<LoggingInterceptor>(t => t == typeof(OrderService));
```

## Example

This example shows attribute-driven logging interception. A `[Logging]` attribute controls which classes and methods get intercepted. The interceptor logs all arguments before the call and the return value after.

### The attribute

```csharp [LoggingAttribute.cs]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class LoggingAttribute : Attribute {}
```

### The interceptor

The interceptor checks whether the invoked method or its declaring class has `[Logging]` applied. If neither does, it skips logging and just proceeds.

```csharp [LoggingInterceptor.cs]
public class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : IInterceptor, ISingletonService
{
    public async Task InterceptAsync(IInterceptorContext context)
    {
        var method = context.Method;
        var hasAttribute =
            method.IsDefined(typeof(LoggingAttribute), inherit: true) ||
            method.DeclaringType!.IsDefined(typeof(LoggingAttribute), inherit: true);

        if (!hasAttribute)
        {
            await context.ProceedAsync();
            return;
        }

        var args = context.Arguments
            .Select((a, i) => $"{method.GetParameters()[i].Name}: {a}")
            .ToArray();

        logger.LogInformation(
            "[{Type}.{Method}] called with ({Args})",
            method.DeclaringType!.Name,
            method.Name,
            string.Join(", ", args));

        await context.ProceedAsync();

        logger.LogInformation(
            "[{Type}.{Method}] returned {ReturnValue}",
            method.DeclaringType!.Name,
            method.Name,
            context.ReturnValue);
    }
}
```

### The service

`[Logging]` is applied at the class level here, so every method on `OrderService` is logged. You can also apply it at the method level to log only specific methods.

::: code-group

```csharp [IOrderService.cs]
public interface IOrderService : ITransientService
{
    Task<Order> GetOrderAsync(int id);
    Task<Order> CreateOrderAsync(string product, int quantity);
}
```

```csharp [OrderService.cs]
[Logging]
public class OrderService : IOrderService
{
    public Task<Order> GetOrderAsync(int id)
        => Task.FromResult(new Order(id, "Unknown", 0));

    public Task<Order> CreateOrderAsync(string product, int quantity)
        => Task.FromResult(new Order(Random.Shared.Next(), product, quantity));
}

public record Order(int Id, string Product, int Quantity);
```

:::

### Registration

The predicate checks whether the implementation type itself or any of its methods has `[Logging]`. This way a single `AddInterceptor` call covers both class-level and method-level usage.

```csharp [MyAppPackage.cs]
internal sealed class MyAppPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddInterceptor<LoggingInterceptor>(t =>
            t.IsDefined(typeof(LoggingAttribute), inherit: true) ||
            t.GetMethods().Any(m => m.IsDefined(typeof(LoggingAttribute), inherit: true)));

        return ValueTask.CompletedTask;
    }
}
```

With this setup, resolving `IOrderService` gives a proxy. Calling `GetOrderAsync(42)` produces log output like:

```
[OrderService.GetOrderAsync] called with (id: 42)
[OrderService.GetOrderAsync] returned Order { Id = 42, Product = Unknown, Quantity = 0 }
```

It is worth understanding exactly what gets proxied and what gets intercepted:

- **No attribute anywhere on the class or its methods**: the predicate returns `false`, so `ServiceInterceptorBinder` never replaces the service descriptor. The resolved instance is the plain `OrderService`, no proxy involved.
- **`[Logging]` on the class**: the predicate returns `true`, so every method on that class gets a proxy. Every invocation enters `InterceptAsync`, but since the attribute check passes for all methods at class level, all of them are logged.
- **`[Logging]` on a specific method only**: the predicate still returns `true` because `t.GetMethods().Any(...)` matches, so the class gets a proxy. But inside `InterceptAsync`, only the method that actually has `[Logging]` passes the attribute check. All other methods proceed immediately without logging.

## Limitations

- Only services registered with an **interface** as the service type are intercepted. Services registered as a concrete class are silently skipped even if the predicate matches.
```csharp
services.AddTransient<OrderService>(); // ✗ skipped, service type is not an interface
services.AddTransient<IOrderService, OrderService>(); // ✓ intercepted, service type is an interface
```
- Services registered via a factory delegate or an existing instance are not intercepted. The predicate only matches services that have a concrete `ImplementationType`.
```csharp
  services.AddTransient<IOrderService>(_ => new OrderService()); // skipped, registred as factory
  services.AddSingleton<IOrderService>(new OrderService()); // skipped, registered as instance
```