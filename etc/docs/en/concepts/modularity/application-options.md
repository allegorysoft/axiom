---
title: Application Options
description: Customizing the Axiom application bootstrap through AxiomApplicationOptions.
---

# Application Options

You can customize how Axiom builds your application by passing an options callback to `ConfigureApplicationAsync`. All options have defaults, so you only need to set what you actually want to change.

```csharp
await builder.ConfigureApplicationAsync(options =>
{
    options.StartupAssembly = typeof(MyApp).Assembly;
});
```

## StartupAssembly

The assembly Axiom uses as the root for dependency graph discovery. Defaults to `Assembly.GetEntryAssembly()`, which is the right choice for most applications.

Override it when you need to point discovery at a different assembly, such as in test projects or multi-application setups.

```csharp
await builder.ConfigureApplicationAsync(options =>
{
    options.StartupAssembly = typeof(MyModule).Assembly;
});
```

::: warning
If `StartupAssembly` is null and `Assembly.GetEntryAssembly()` also returns null, an `ArgumentNullException` is thrown at startup.
:::

## DependencyRegistrar

Controls how services are scanned and registered into `IServiceCollection`. Defaults to a standard `AssemblyDependencyRegistrar` backed by the host's service collection.

Override it by subclassing `AssemblyDependencyRegistrar` and providing your own instance. This is useful when you need custom discovery or registration behavior.

```csharp
public class MyRegistrar(IServiceCollection services) : AssemblyDependencyRegistrar(services)
{
    protected override IEnumerable<Type> GetImplementationTypes(Assembly assembly)
    {
        return base.GetImplementationTypes(assembly)
            .Where(t => !t.Namespace!.Contains(".Internal"));
    }
}

await builder.ConfigureApplicationAsync(options =>
{
    options.DependencyRegistrar = new MyRegistrar(builder.Services);
});
```

See [Dependency Injection](./concepts/dependency-injection) for the full `AssemblyDependencyRegistrar` API.

## ApplicationBuilder

Controls how the application is built. Defaults to a standard `AxiomApplicationBuilder` which handles assembly resolution, lifecycle hook invocation, and `AxiomApplication` construction.

Override it by subclassing `AxiomApplicationBuilder`. All discovery and invocation steps are virtual, so you can replace only what you need.

```csharp
public class MyApplicationBuilder : AxiomApplicationBuilder
{
    protected override IEnumerable<Assembly> GetDependencies()
    {
        var assemblies = base.GetDependencies().ToList();
        assemblies.Add(typeof(SomeExtraAssembly).Assembly);
        return assemblies;
    }
}

await builder.ConfigureApplicationAsync(options =>
{
    options.ApplicationBuilder = new MyApplicationBuilder();
});
```

### Virtual methods

| Method | Description |
|---|---|
| `BuildAsync()` | Orchestrates the full build. Calls `GetDependencies`, `GetPlugins`, `ConfigureApplicationAsync`, `PostConfigureApplicationAsync`, and constructs `AxiomApplication`. |
| `GetDependencies()` | Loads `DependencyContext` and returns all qualifying assemblies. Falls back to `GetReferencedAssemblies()` when no context is available. |
| `GetDependencies(DependencyContext)` | Filters `RuntimeLibraries` by transitive dependency on `Allegory.Axiom.DependencyInjection.Abstractions`. |
| `GetPlugins()` | Returns assemblies from all registered plugins. |
| `ConfigureApplicationAsync(assemblies)` | Runs DI scanning and calls `IConfigureApplication.ConfigureAsync` on each assembly. |
| `PostConfigureApplicationAsync(assemblies)` | Calls `IPostConfigureApplication.PostConfigureAsync` on each assembly. |

## Plugins

A list of `IAxiomApplicationPlugin` instances for loading assemblies that are not part of the dependency graph. Defaults to an empty list.

```csharp
await builder.ConfigureApplicationAsync(options =>
{
    options.Plugins.Add(new AxiomApplicationFilePlugin("/path/to/plugin.dll"));
});
```

See [Plugins](./plugins) for the full plugin API.
