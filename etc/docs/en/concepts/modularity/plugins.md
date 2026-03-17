---
title: Plugins
description: Loading assemblies into Axiom that are not part of the dependency graph.
---

# Plugins

Axiom discovers assemblies by walking the dependency graph of the startup assembly. Plugins are for cases where you need to include assemblies that sit outside that graph, such as optional modules loaded at runtime, third-party extensions shipped as separate binaries, or assemblies conditionally loaded based on configuration.

Plugins are registered through `AxiomApplicationOptions` and are appended to the assembly list after all dependency graph assemblies have been resolved.

```csharp
await builder.ConfigureApplicationAsync(options =>
{
    options.Plugins.Add(new AxiomApplicationFilePlugin("/path/to/plugin.dll"));
});
```

Axiom includes three built-in plugin types. You can also implement `IAxiomApplicationPlugin` for custom loading scenarios.

## AxiomApplicationFilePlugin

Loads a single assembly from a file path.

```csharp
await builder.ConfigureApplicationAsync(options =>
{
    options.Plugins.Add(new AxiomApplicationFilePlugin("/plugins/MyPlugin.dll"));
});
```

The assembly is loaded immediately when the plugin is constructed using `AssemblyLoadContext.Default`.

## AxiomApplicationDirectoryPlugin

Loads all `.dll` and `.exe` files found in a directory.

```csharp
await builder.ConfigureApplicationAsync(options =>
{
    options.Plugins.Add(new AxiomApplicationDirectoryPlugin("/plugins"));
});
```

By default it scans recursively. Pass `recursive: false` to limit scanning to the top-level directory only.

```csharp
options.Plugins.Add(new AxiomApplicationDirectoryPlugin("/plugins", recursive: false));
```

All matching files are loaded at construction time using `AssemblyLoadContext.Default`. Duplicate paths are deduplicated before loading.

## AxiomApplicationAssemblyPlugin

Loads one or more assemblies that are already in memory. Use this when you have a reference to the assembly directly, such as in tests or when the assembly is loaded through other means.

```csharp
await builder.ConfigureApplicationAsync(options =>
{
    options.Plugins.Add(new AxiomApplicationAssemblyPlugin(
        typeof(PluginA.PluginAPackage).Assembly,
        typeof(PluginB.PluginBPackage).Assembly
    ));
});
```

## Custom Plugins

Implement `IAxiomApplicationPlugin` to load assemblies from any source, such as a remote location, a database, or a custom package format.

```csharp
public interface IAxiomApplicationPlugin
{
    IEnumerable<Assembly> GetAssemblies();
}
```

```csharp
public class MyCustomPlugin : IAxiomApplicationPlugin
{
    public IEnumerable<Assembly> GetAssemblies()
    {
        // Load assemblies from any source
        yield return AssemblyLoadContext.Default.LoadFromAssemblyPath(
            ResolvePluginPath());
    }

    private string ResolvePluginPath()
    {
        // Your custom resolution logic
    }
}

await builder.ConfigureApplicationAsync(options =>
{
    options.Plugins.Add(new MyCustomPlugin());
});
```

::: warning
Plugin assemblies are loaded at the time `ConfigureApplicationAsync` is called. If a file does not exist or cannot be loaded, an exception will be thrown during startup.
:::
