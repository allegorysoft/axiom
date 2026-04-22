# Axiom

> ⚠️ This project is in early alpha and not yet recommended for production use. APIs may change without notice.

Axiom is an open source .NET application framework for building modular applications. It handles the foundational infrastructure so you can focus on application logic instead of building common infrastructure yourself.

## Features

- **Automatic Dependency Injection** Discover and register services by scanning assemblies via marker interfaces (`ITransientService`, `IScopedService`, `ISingletonService`) or `[Dependency]` attributes. No manual `services.Add*()` calls needed.
- **Modularity & Plugin System** Compose your application from self-contained modules with ordered lifecycle hooks (`IConfigureApplication`, `IPostConfigureApplication`, `IInitializeApplication`). Load plugin assemblies at runtime without recompiling.
- **AOP Interception** Apply logging, caching, authorization, or transactions transparently via a Castle DynamicProxy-backed pipeline. Zero changes to your business logic.
- **Unit of Work** Manage transactional boundaries declaratively with `IUnitOfWorkScope` or `[UnitOfWork]`. Nests naturally across service boundaries through `AsyncLocal` ambient context.
- **File Providers** Compose multiple file sources (embedded, physical, custom) into a single virtual file system via `FileProviderManager`.
- **Localization** JSON-based, file provider driven string localization with automatic culture fallback and runtime update support.

## Requirements

- .NET 10.0+

## Installation

Install only the packages you need:

```bash
# Core DI and service registration
dotnet add package Allegory.Axiom.DependencyInjection.Abstractions

# Hosting lifecycle and modularity
dotnet add package Allegory.Axiom.Hosting.Abstractions

# AOP interception
dotnet add package Allegory.Axiom.Interception.Abstractions
dotnet add package Allegory.Axiom.Interception.Castle.Core

# Unit of Work
dotnet add package Allegory.Axiom.UnitOfWork.Abstractions
dotnet add package Allegory.Axiom.UnitOfWork

# File Providers
dotnet add package Allegory.Axiom.FileProviders

# Localization
dotnet add package Allegory.Axiom.Localization.Abstractions
dotnet add package Allegory.Axiom.Localization
```

## Quick Start

### Minimal Host Setup

```csharp
var builder = Host.CreateApplicationBuilder(args);
await builder.ConfigureApplicationAsync();
var host = builder.Build();
await host.InitializeApplicationAsync();
host.Run();
```

### Automatic Service Registration

Implement a marker interface and your service is registered automatically:

```csharp
public class OrderService : IOrderService, ITransientService { }
public class UserSession  : IUserSession,  IScopedService    { }
public class AppConfig    : IAppConfig,    ISingletonService { }
```

### Application Packages

Group assembly-level configuration in a package class:

```csharp
internal sealed class MyAppPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<MyOptions>(
            builder.Configuration.GetSection("MyOptions"));
        builder.Services.AddHostedService<MyBackgroundWorker>();
        return ValueTask.CompletedTask;
    }
}
```

Package classes are discovered automatically no registration required.

### Interception

Write an interceptor once and apply it across any matching services:

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

Mark a service interface with `IUnitOfWorkScope` to wrap every method in a transaction automatically:

```csharp
public interface IOrderService : IUnitOfWorkScope, ITransientService
{
    Task PlaceOrderAsync(Order order);  // Required transaction
    Task<Order> GetOrderAsync(int id);  // Suppress (read-prefix heuristic)
}
```

Or use `[UnitOfWork]` for explicit per-class or per-method control:

```csharp
[UnitOfWork]
internal sealed class OrderService : IOrderService
{
    public Task PlaceOrderAsync(Order order) { ... }

    [UnitOfWork(UnitOfWorkTransactionBehavior.RequiresNew)]
    public Task CompensateAsync() { ... }

    [UnitOfWork(false)]
    public Task AuditOnlyAsync() { ... }
}
```

### Plugins

Load assemblies that are not part of the dependency graph at runtime:

```csharp
await builder.ConfigureApplicationAsync(options =>
{
    options.Plugins.Add(new AxiomApplicationDirectoryPlugin("/plugins"));
});
```

## Documentation

Full documentation is available at [axiomframework.dev](https://axiomframework.dev).

Topics covered:

- [Overview](https://axiomframework.dev/get-started/overview)
- [Installation](https://axiomframework.dev/get-started/installation)
- [Dependency Injection](https://axiomframework.dev/concepts/dependency-injection)
- [Modularity](https://axiomframework.dev/concepts/modularity/overview)
  - [Application Options](https://axiomframework.dev/concepts/modularity/application-options)
  - [Plugins](https://axiomframework.dev/concepts/modularity/plugins)
- [Interception](https://axiomframework.dev/concepts/interception)
- [Unit of Work](https://axiomframework.dev/concepts/unit-of-work)
- [File Providers](https://axiomframework.dev/concepts/file-providers)
- [Localization](https://axiomframework.dev/concepts/localization)

## License

Open source packages are licensed under [MIT](etc/licenses/MIT.md).  
Enterprise packages are licensed under [ELv2](etc/licenses/ELv2.md) with a [Commercial License](etc/licenses/COMMERCIAL_LICENSE.md) available.