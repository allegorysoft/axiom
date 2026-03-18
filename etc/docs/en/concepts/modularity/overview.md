---
title: Modularity
description: How Axiom discovers assemblies, wires up packages, and manages the application lifecycle.
---

# Modularity

Axiom modularity is built around two things: automatic assembly discovery and application packages. When you call `ConfigureApplicationAsync`, Axiom walks your dependency graph, scans each discovered assembly for a package class, invokes the appropriate lifecycle hooks, and registers your services.

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

Most of the time, you do not need a package class at all. Axiom automatically scans every discovered assembly with `AssemblyDependencyRegistrar`, so any class marked with a marker interface or a `[Dependency]` attribute is registered without any additional wiring. See [Dependency Injection](../dependency-injection) for details.

A package class is only needed when you require something beyond automatic registration, such as configuring the options pattern, registering hosted services, or setting up third-party integrations.

```csharp
// No package class needed for straightforward service registration
public class OrderService : IOrderService, ITransientService { }

// A package class is needed for things like options configuration
internal sealed class MyLibraryPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<MyLibraryOptions>(
            builder.Configuration.GetSection("MyLibrary"));

        builder.Services.AddHostedService<MyBackgroundWorker>();
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

**`IPostConfigureApplication`** runs after every assembly's `ConfigureAsync` has finished. Because `ConfigureAsync` runs per assembly and the order follows `deps.json` (see [Assembly Discovery](#assembly-discovery)), you cannot guarantee that a specific assembly has already configured its services when your own `ConfigureAsync` runs. If your setup depends on another assembly having already registered its services, such as replacing or decorating a service registered elsewhere, use `IPostConfigureApplication` instead. By the time `PostConfigureAsync` runs, all `ConfigureAsync` calls across all assemblies have completed.

```csharp
internal sealed class MyAppPackage : IPostConfigureApplication
{
    public static ValueTask PostConfigureAsync(IHostApplicationBuilder builder)
    {
        // Safe to replace here, all assemblies have already run ConfigureAsync
        builder.Services.Replace<IOrderService, ReplacedOrderManager>();
        return ValueTask.CompletedTask;
    }
}
```

**`IInitializeApplication`** receives the built `IHost`. Use it for anything that requires a fully running service provider, such as seeding a database, warming up a cache, or running startup validation.

```csharp
internal sealed class MyAppPackage : IInitializeApplication
{
    public static async ValueTask InitializeAsync(IHost host)
    {
        var seeder = host.Services.GetRequiredService<IDatabaseSeeder>();
        await seeder.SeedAsync();
    }
}
```

## Assembly Discovery

`ConfigureApplicationAsync` loads the [`DependencyContext`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencymodel.dependencycontext) of the startup assembly, which is generated from the `deps.json` file produced at publish time. It iterates through `RuntimeLibraries` and includes any library that has a transitive dependency on `Allegory.Axiom.DependencyInjection.Abstractions`. This means your own libraries are picked up automatically as long as they reference Axiom.

The assembly iteration order follows the order of entries in `deps.json`. Axiom does not impose any additional ordering on top of that.

In a typical host application, you do not need to think about which assemblies get included. Any project in your solution that references Axiom will be discovered transitively through the dependency graph.

```
HostApp
  └── OrderModule        (references Axiom → discovered)
        └── SharedKernel (references Axiom → discovered)
```

Plugin assemblies are appended after all dependency graph assemblies. See [Plugins](./plugins) for loading assemblies that are not part of the dependency graph.

### Fallback: GetReferencedAssemblies

If no `DependencyContext` is available, such as in some single-file or trimmed publish scenarios, Axiom falls back to [`Assembly.GetReferencedAssemblies()`](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly.getreferencedassemblies) on the startup assembly.

::: warning Known limitation
The .NET compiler omits a referenced assembly from metadata if none of its types are directly used in the referencing project. In that case, `GetReferencedAssemblies()` will not return it, and Axiom will not discover it.

This is a [known issue](https://github.com/allegorysoft/axiom/issues/6). A CLI command is planned that will generate a `ReferenceHolder` class in your startup project, explicitly referencing at least one type from each Axiom-dependent assembly to prevent the compiler from dropping them.

If you run into this today, the workaround is to add a direct type reference from the missing assembly somewhere in your startup project.
:::

## Host Extensions

Two extension methods drive the full lifecycle.

### ConfigureApplicationAsync

Call this on `IHostApplicationBuilder` before building the host. It discovers assemblies, runs DI scanning via `AssemblyDependencyRegistrar`, calls `ConfigureAsync` and `PostConfigureAsync` on each discovered package, executes any queued post-configure actions, and registers the `AxiomApplication` singleton into the container.

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
