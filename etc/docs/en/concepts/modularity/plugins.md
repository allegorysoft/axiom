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
options.Plugins.Add(new AxiomApplicationFilePlugin("/plugins/MyPlugin.dll"));
```

The assembly is loaded immediately when the plugin is constructed using `AssemblyLoadContext.Default`.

## AxiomApplicationDirectoryPlugin

Loads all `.dll` and `.exe` files found in a directory.

```csharp
options.Plugins.Add(new AxiomApplicationDirectoryPlugin("/plugins"));
```

By default it scans recursively. Pass `recursive: false` to limit scanning to the top-level directory only.

```csharp
options.Plugins.Add(new AxiomApplicationDirectoryPlugin("/plugins", recursive: false));
```

All matching files are loaded at construction time using `AssemblyLoadContext.Default`. Duplicate paths are deduplicated before loading.

## AxiomApplicationAssemblyPlugin

Loads one or more assemblies that are already in memory. Use this when you have a direct reference to the assembly, such as in tests or when the assembly is loaded through other means.

```csharp
options.Plugins.Add(new AxiomApplicationAssemblyPlugin(
    typeof(PluginA.PluginAPackage).Assembly,
    typeof(PluginB.PluginBPackage).Assembly
));
```

## Custom Plugins

Implement `IAxiomApplicationPlugin` to load assemblies from any source.

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
        yield return AssemblyLoadContext.Default.LoadFromAssemblyPath(
            ResolvePluginPath());
    }

    private string ResolvePluginPath()
    {
        // Your custom resolution logic
    }
}

options.Plugins.Add(new MyCustomPlugin());
```

::: warning
Plugin assemblies are loaded at the time `ConfigureApplicationAsync` is called. If a file does not exist or cannot be loaded, an exception will be thrown during startup.
:::

## Example

This example walks through a host application that loads a command plugin at runtime. The plugin is not referenced by the host project, so it would never be discovered through the dependency graph. Plugins are the right tool for exactly this case.

The example has three projects with the following dependency structure:

```
MyApp.Host        (references MyApp, Microsoft.Extensions.Hosting)
  └── MyApp       (references Allegory.Axiom.Hosting.Abstractions)

MyApp.Plugin      (references MyApp)
```

`MyApp.Plugin` is not referenced by `MyApp.Host`, so it will never be discovered through the dependency graph. The host loads it explicitly as a plugin at runtime.

### Base application

The base application defines an `ICommand` interface and a `CommandManager` that holds registered commands. The package registers `CommandManager` as a singleton.

```bash
dotnet add package Allegory.Axiom.Hosting.Abstractions
```

::: code-group

```csharp [ICommand.cs]
public interface ICommand
{
    string Key { get; }
    void Execute();
}
```

```csharp [CommandManager.cs]
public class CommandManager : ISingletonService
{
    private readonly List<ICommand> _commands = [];

    public void Register(ICommand command) => _commands.Add(command);

    public bool Run(string key)
    {
        var command = _commands.FirstOrDefault(c => c.Key == key);
        if (command is null) return false;
        command.Execute();
        return true;
    }
}
```

```csharp [MyAppPackage.cs]
internal sealed class MyAppPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        // CommandManager is registered automatically via ISingletonService,
        // but you can do additional setup here if needed.
        return ValueTask.CompletedTask;
    }
}
```

:::

### Plugin

The plugin project references `MyApp` but is not referenced back. It does not need to install `Allegory.Axiom.Hosting.Abstractions` directly since it comes transitively through `MyApp`. It implements `ICommand` and registers itself into `CommandManager` during initialization.

::: code-group

```csharp [HelloCommand.cs]
public class HelloCommand : ICommand
{
    public string Key => "hello";
    public void Execute() => Console.WriteLine("Hello from plugin!");
}
```

```csharp [MyPluginPackage.cs]
internal sealed class MyPluginPackage : IInitializeApplication
{
    public static ValueTask InitializeAsync(IHost host)
    {
        var manager = host.Services.GetRequiredService<CommandManager>();
        manager.Register(new HelloCommand());
        return ValueTask.CompletedTask;
    }
}
```

:::

`IInitializeApplication` is used here because `CommandManager` needs to be resolved from the built service provider, which is only available after the host is built.

### Host

The host references `MyApp` and installs `Microsoft.Extensions.Hosting` to bootstrap the application. `Allegory.Axiom.Hosting.Abstractions` comes transitively through `MyApp`.

```bash
dotnet add package Microsoft.Extensions.Hosting
```

It loads all assemblies from a plugins directory using `AxiomApplicationDirectoryPlugin`. The plugin's `IInitializeApplication` is called during `InitializeApplicationAsync`, at which point `HelloCommand` is registered into `CommandManager`.

::: code-group

```csharp [Program.cs]
var builder = Host.CreateApplicationBuilder(args);

await builder.ConfigureApplicationAsync(options =>
{
    options.Plugins.Add(new AxiomApplicationDirectoryPlugin("/plugins"));
});

var host = builder.Build();
await host.InitializeApplicationAsync();

var manager = host.Services.GetRequiredService<CommandManager>();

while (true)
{
    Console.Write("Enter command: ");
    var input = Console.ReadLine();

    if (!manager.Run(input!))
    {
        Console.WriteLine("No command found.");
    }
}
```

:::

The base application has no knowledge of the plugin. The plugin knows about the base application. The host wires them together at startup by pointing to the plugins directory.
