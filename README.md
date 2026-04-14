# Axiom

> ⚠️ This project is currently under active development and not yet ready for production use.

Axiom is an open source .NET application framework for building modular applications. It covers common cross-cutting concerns dependency injection, modularity, hosting lifecycle, AOP-style interception, and transaction management letting you focus on your application logic instead of infrastructure boilerplate.

## Features

- **Reflection-based DI** automatically discover and register services by scanning assemblies via marker interfaces (`ITransientService`, `IScopedService`, `ISingletonService`) or `[Dependency]` attributes. No manual wiring needed.
- **Modularity** compose your application from self-contained modules with ordered lifecycle hooks (`IConfigureApplication`, `IPostConfigureApplication`, `IInitializeApplication`). Drop in plugin assemblies at runtime without recompiling.
- **Interception** apply logging, caching, authorization, or transactions transparently via a Castle DynamicProxy-backed AOP pipeline. Zero changes to your business logic.
- **Unit of Work** manage transactional boundaries declaratively with `IUnitOfWorkScope` or `[UnitOfWork]` attribute. Nests naturally across service boundaries through `AsyncLocal` ambient context.

## Getting Started

### Minimal Application

```bash
dotnet add package Allegory.Axiom.Hosting.Abstractions
```

```csharp
var builder = Host.CreateApplicationBuilder(args);
await builder.ConfigureApplicationAsync();
var host = builder.Build();
await host.InitializeApplicationAsync();
host.Run();
```

### Automatic Service Registration

Any class implementing a marker interface is registered automatically no `services.Add*()` calls needed.

```csharp
public class OrderService : IOrderService, ITransientService { }

public class UserSession : IUserSession, IScopedService { }

public class AppConfig : IAppConfig, ISingletonService { }
```

### Modularity

Use application packages to group configuration per assembly.

```csharp
internal sealed class MyAppPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<MyBackgroundWorker>();
        return ValueTask.CompletedTask;
    }
}
```

### Interception

Write an interceptor once, apply it across any matching service no changes to business logic required.

```csharp
public class LoggingInterceptor : IInterceptor, ISingletonService
{
    public async Task InterceptAsync(IInterceptorContext context)
    {
        Console.WriteLine($"Calling {context.Method.Name}");
        await context.ProceedAsync();
        Console.WriteLine($"Finished {context.Method.Name}");
    }
}

// Register in your package
builder.Services.AddInterceptor<LoggingInterceptor>(
    t => typeof(IOrderService).IsAssignableFrom(t));
```

### Unit of Work

Mark a service interface with `IUnitOfWorkScope` to wrap every method in a transaction automatically.

```csharp
public interface IOrderService : IUnitOfWorkScope, ITransientService
{
    Task PlaceOrderAsync(Order order);  // → Required transaction
    Task<Order> GetOrderAsync(int id); // → Suppress (read prefix heuristic)
}
```

## Documentation

Full documentation is available at [axiomframework.dev](https://axiomframework.dev).

## License

Open source packages are licensed under [MIT](etc/licenses/MIT.md).  
Enterprise packages are licensed under [ELv2](etc/licenses/ELv2.md) with a [Commercial License](etc/licenses/COMMERCIAL_LICENSE.md) available.