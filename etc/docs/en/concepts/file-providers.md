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

Providers are registered in order, but resolved in **reverse order** the last registered provider has highest priority.

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
        var contents = FileProvider.GetDirectoryContents("/");
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

## File Provider Options

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

## Embedded Provider Behavior

When using `AddEmbedded` to register embedded resources, the provider uses [ManifestEmbeddedFileProvider](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.fileproviders.manifestembeddedfileprovider) under the hood. This requires your project to be configured correctly so that a manifest file is generated at build time without it, the provider cannot discover or serve any embedded files.

### Required Project Configuration

Add the following to your `.csproj` file in the assembly that contains the embedded resources:

**1. Enable manifest generation**

The `GenerateEmbeddedFilesManifest` property instructs the build system to produce the manifest file that `ManifestEmbeddedFileProvider` reads at runtime to locate embedded resources.

```xml
<PropertyGroup>
    <GenerateEmbeddedFilesManifest>true</GenerateEmbeddedFilesManifest>
</PropertyGroup>
```

**2. Add the embedded file provider package**

This package supplies `ManifestEmbeddedFileProvider` itself, and its build targets are responsible for generating and embedding the manifest into the output assembly.

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.Extensions.FileProviders.Embedded" Version="10.0.0" />
</ItemGroup>
```

**3. Mark your files as embedded resources**

By default, files inside your project are not embedded. You must explicitly include them as `EmbeddedResource` items. The example below embeds everything under a `Resources/` folder:

```xml
<ItemGroup>
    <None Remove="Resources\**" />
    <EmbeddedResource Include="Resources\**" />
</ItemGroup>
```

Now you can use `AddEmbedded` in your `FileProviderOptions` to register these embedded resources, and they will be discoverable at runtime through the file provider system.

## File Provider Manager

`FileProviderManager` is the central entry point for all file resolution in Axiom. You can be injected and use it anywhere in your application to access the virtual file system composed of all registered providers.

Internally, it wraps a `CompositeFileProvider` built from all providers registered through `FileProviderOptions`. Rather than exposing its own resolution logic, it delegates every call `GetFileInfo`, `GetDirectoryContents`, and `Watch` to that composite.

### Extensibility

`GetFileInfo`, `GetDirectoryContents`, and `Watch` are all declared `virtual`, so you can subclass `FileProviderManager` to intercept or augment resolution behavior for example, to add path rewriting, logging, or access control without replacing the entire provider pipeline.