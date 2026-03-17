---
title: Modularity
description: How Axiom discovers assemblies, wires up packages, and manages the application lifecycle.
---

# Modularity

Axiom modularity is built around two things: automatic assembly discovery and application packages. When you call `ConfigureApplicationAsync`, Axiom walks your dependency graph, scans each discovered assembly for a package class, invokes the appropriate lifecycle hooks, and registers your services. You get a fully wired application without manually enumerating packages.

Install the hosting package to get started:

```bash
dotnet add package Allegory.Axiom.Hosting.Abstractions
```

## Application Packages

An application package is a class that implements one or more of the lifecycle interfaces: `IConfigureApplication`, `IPostConfigureApplication`, and `IInitializeApplication`. Axiom discovers it by scanning each assembly in the dependency graph and calls the appropriate static methods at each phase.

You do not register packages anywhere. Having one in an assembly that is part of the dependency graph is enough for Axiom to find it.

```csharp
internal sealed class MyAppPackage : IConfigureApplication, IPostConfigureApplication, IInitializeApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IMyService, MyService>();
        return ValueTask.CompletedTask;
    }

    public static ValueTask PostConfigureAsync(IHostApplicationBuilder builder)
    {
        // Runs after all ConfigureAsync calls have completed across all assemblies
        return ValueTask.CompletedTask;
    }

    public static ValueTask InitializeAsync(IHost host)
    {
        // Runs after the host is built
        return ValueTask.CompletedTask;
    }
}
```

::: warning
Each assembly can contain at most one class implementing a given lifecycle interface. Having more than one will throw an `InvalidOperationException` at startup.
:::

You do not have to implement all three interfaces. Implement only what your assembly needs.

```csharp
// A library that only needs to register services
internal sealed class MyLibraryPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<IOrderService, OrderService>();
        return ValueTask.CompletedTask;
    }
}
```

## Lifecycle Interfaces

| Interface | Method | When it runs |
|---|---|---|
| `IConfigureApplication` | `ConfigureAsync(IHostApplicationBuilder)` | During `ConfigureApplicationAsync`, once per assembly |
| `IPostConfigureApplication` | `PostConfigureAsync(IHostApplicationBuilder)` | After all `ConfigureAsync` calls have completed |
| `IInitializeApplication` | `InitializeAsync(IHost)` | During `InitializeApplicationAsync`, after the host is built |

**`IConfigureApplication`** is the right place for DI registration, configuration binding, and anything else that sets up the container.

**`IPostConfigureApplication`** runs after every assembly's `ConfigureAsync` has finished. Use it when your setup depends on other assemblies having already registered their services, such as decorating a service registered elsewhere.

**`IInitializeApplication`** receives the built `IHost`. Use it for anything that requires a fully running service provider, such as seeding a database, warming up a cache, or running startup validation.

## Assembly Discovery

`ConfigureApplicationAsync` loads the `DependencyContext` of the startup assembly (the `deps.json` produced at publish time) and iterates through `RuntimeLibraries`. Any library that has a transitive dependency on `Allegory.Axiom.DependencyInjection.Abstractions` is included.

The assembly iteration order follows the order of entries in `deps.json`. Axiom does not impose any additional ordering on top of that. If no `DependencyContext` is available, such as in some single-file or trimmed publish scenarios, Axiom falls back to `Assembly.GetReferencedAssemblies()` on the startup assembly.

Plugin assemblies are appended after all dependency graph assemblies. See [Plugins](./plugins) for loading assemblies that are not part of the dependency graph.

## Host Extensions

Two extension methods on `IHostApplicationBuilder` and `IHost` drive the full lifecycle.

### ConfigureApplicationAsync

Call this on your `IHostApplicationBuilder` before building the host. It runs the full configure phase: assembly discovery, DI scanning via `AssemblyDependencyRegistrar`, `ConfigureAsync` and `PostConfigureAsync` on each discovered package, and post-configure actions. It also registers the `AxiomApplication` singleton into the container.

```csharp
var builder = Host.CreateApplicationBuilder(args);
await builder.ConfigureApplicationAsync();
var host = builder.Build();
```

By default the startup assembly is `Assembly.GetEntryAssembly()`. You can override this and other defaults via `AxiomApplicationOptions`. See [Application Options](./application-options).

### InitializeApplicationAsync

Call this on `IHost` after the host is built. It resolves `AxiomApplication` from DI and calls `InitializeAsync` on every package found across all discovered assemblies.

```csharp
await host.InitializeApplicationAsync();
host.Run();
```

A complete host setup looks like this:

```csharp
var builder = Host.CreateApplicationBuilder(args);
await builder.ConfigureApplicationAsync();
var host = builder.Build();
await host.InitializeApplicationAsync();
host.Run();
```

## AxiomApplication

`AxiomApplication` is registered as a singleton by `ConfigureApplicationAsync` and can be injected anywhere in your application. It exposes the resolved assembly list and the startup assembly.

```csharp
public sealed class AxiomApplication
{
    public Guid Id { get; }
    public Assembly StartupAssembly { get; }
    public IReadOnlyCollection<Assembly> Assemblies { get; }
}
```

| Property | Description |
|---|---|
| `Id` | A unique identifier generated for this application instance. |
| `StartupAssembly` | The assembly used as the root for dependency discovery. |
| `Assemblies` | All assemblies included in the application, in discovery order. |
