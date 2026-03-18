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

`Allegory.Axiom.Interception.Abstractions` provides the core interfaces and registration API. `Allegory.Axiom.Interception.Castle.Core` provides the [Castle DynamicProxy](https://www.castleproject.org/projects/dynamicproxy/) implementation that creates the actual proxies. You need both.

## Writing an Interceptor

Implement `IAxiomInterceptor` with a single `InterceptAsync` method. Call `context.ProceedAsync()` to invoke the next interceptor in the chain or the actual method.

```csharp
public class LoggingInterceptor : IAxiomInterceptor, ISingletonService
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public async Task InterceptAsync(IAxiomInterceptorContext context)
    {
        _logger.LogInformation("Calling {Method}", context.Method.Name);
        await context.ProceedAsync();
        _logger.LogInformation("Finished {Method}", context.Method.Name);
    }
}
```

Interceptors are resolved from the DI container, so constructor injection works normally. Register them using a DI marker interface or `[Dependency]` attribute just like any other service.

## IAxiomInterceptorContext

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
public async Task InterceptAsync(IAxiomInterceptorContext context)
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

Register interceptors via `services.AddInterceptor<T>(predicate)` in your package's `ConfigureAsync`. The predicate receives the implementation type of each registered service and returns `true` for the ones you want to intercept.

```csharp
internal sealed class MyAppPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        // Intercept all services that implement IOrderService
        builder.Services.AddInterceptor<LoggingInterceptor>(
            t => typeof(IOrderService).IsAssignableFrom(t));

        // Intercept a specific type
        builder.Services.AddInterceptor<CachingInterceptor>(
            t => t == typeof(ProductRepository));

        return ValueTask.CompletedTask;
    }
}
```

`AddInterceptor` does not apply proxies immediately. It queues the registration. At the end of `ConfigureApplicationAsync`, Axiom runs `ServiceInterceptorBinder.Apply` as a post-configure action, which replaces all matched service descriptors with proxy factories.

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
public class SingletonInterceptor : IAxiomInterceptor, ISingletonService { ... }
public class ScopedInterceptor    : IAxiomInterceptor, IScopedService    { ... }
public class TransientInterceptor : IAxiomInterceptor, ITransientService { ... }
```

The proxy preserves the lifetime of the original service descriptor. Registering a transient service with a scoped interceptor does not change the service's lifetime it still resolves as transient.

::: warning Lifetime mismatch
If an interceptor has a longer lifetime than a service it depends on, you will get a captive dependency. For example, a scoped interceptor that injects a transient service holds onto that transient instance for the entire scope instead of getting a fresh one per resolution. Follow the standard .NET DI rule: a service should never depend on something with a shorter lifetime.
:::

## Interface vs Class Proxies

Axiom automatically creates the right proxy type based on how the service is registered.

- If the service type is an **interface**, an interface proxy is created.
- If the service type is a **class**, a class proxy is created.

```csharp
// Interface proxy — recommended
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddInterceptor<LoggingInterceptor>(t => t == typeof(OrderService));

// Class proxy — works but requires the class to be non-sealed with virtual methods
builder.Services.AddTransient<OrderService>();
builder.Services.AddInterceptor<LoggingInterceptor>(t => t == typeof(OrderService));
```

::: warning
Class proxies require the intercepted methods to be `virtual`. Non-virtual methods on a class proxy are not intercepted.
:::

## Keyed Services

Interception works with keyed services. The proxy preserves the service key.

```csharp
builder.Services.AddKeyedScoped<IOrderService, OrderService>("primary");
builder.Services.AddInterceptor<LoggingInterceptor>(t => t == typeof(OrderService));
```

## Limitations

- Services registered via a factory delegate or an existing instance are not intercepted. The predicate only matches services that have a concrete `ImplementationType`.
- `ReturnValue` is not accessible for void methods (non-generic context). Accessing it throws `NotSupportedException`.
