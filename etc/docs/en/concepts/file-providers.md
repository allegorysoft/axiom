---
title: File Providers
description: Composable file provider management for virtual file systems in Axiom applications.
---

# File Providers

Axiom file provider system wraps `Microsoft.Extensions.FileProviders` with a composable manager that merges multiple sources into a single virtual file system. Register physical directories, embedded resources, or any custom `IFileProvider` and resolve them all through one manager.

```bash
dotnet add package Allegory.Axiom.FileProviders
```

## Overview

`FileProviderManager` accepts providers via `FileProviderOptions` and merges them into a single [CompositeFileProvider](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.fileproviders.compositefileprovider).

Providers are registered in order, but resolved in **reverse order** the last registered provider has highest priority. This matches the typical override pattern: add defaults first, then environment-specific overrides last.

```csharp
// configure
builder.Services.Configure<FileProviderOptions>(options =>
{
    options.AddEmbedded<MyAppPackage>(); // registered first → lowest priority
    options.AddPhysical(Path.Combine(AppContext.BaseDirectory, "resources")); // registered last  → highest priority
});

// resolve
public class MyService(FileProviderManager fileProvider) : ITransientService
{
    public FileProviderManager FileProvider { get; } = fileProvider;

    public void DoSomething()
    {
        var file = FileProvider.GetFileInfo("data.json");
    }
}
```

## Configuration

Configure providers through the options pattern in your [application package](./modularity/overview#application-packages):

```csharp
internal sealed class MyAppPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<FileProviderOptions>(options =>
        {
            options.Providers.Add(new MyCustomFileProvider());
            options.AddEmbedded<MyAppPackage>();
            options.AddPhysical(AppContext.BaseDirectory);
        });

        return ValueTask.CompletedTask;
    }
}
```

::: warning
Order matters: providers added later override earlier ones. If multiple providers contain the same file, the last registered provider takes precedence and is used to resolve the file.

Example: if a.txt exists in both Embedded and Physical providers, and Physical is registered last, the Physical version of a.txt will be returned.
:::

## `FileProviderOptions`

| Method | Description |
|---|---|
| `AddEmbedded<T>(string? root = null)` | Uses the assembly of `T`. Optional `root` scopes the path. |
| `AddEmbedded(Assembly, string? root = null)` | Uses a specific assembly. |
| `AddPhysical(string root, ExclusionFilters? filters = null)` | Uses a physical directory. |

You can also register custom providers directly:

```csharp
options.Providers.Add(new MyCustomFileProvider());
```

::: tip
See [Microsoft.Extensions.FileProviders](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/file-providers) for built-in providers.
:::

::: details Root parameter behavior (Embedded providers)
When using `AddEmbedded`, the optional `root` parameter controls which embedded resources are exposed and how their paths are resolved.

#### What it does

- Limits exposed resources to a specific namespace/root
- Strips that root prefix from the final virtual path

#### Embedded resource inside assembly

Let's say in a c# project we have embedded file like; `MyApp/Resources/Images/logo.png`

```csharp
options.AddEmbedded<MyApp>("MyApp.Resources");
//Resulting exposed path: "Images/logo.png" instead of "MyApp/Resources/Images/logo.png"
```

Root acts like a “virtual mount point” for embedded resources.
:::