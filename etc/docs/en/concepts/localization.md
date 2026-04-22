---
title: Localization
description: JSON-based, file-provider-driven string localization with culture fallback for Axiom applications.
---

# Localization

Axiom localization wraps `Microsoft.Extensions.Localization` with a JSON based, [file provider](./file-providers) system. Define translation files per culture, register resource types, and resolve strings through the standard `IStringLocalizer<T>` with automatic parent culture fallback and support for dynamic runtime updates.

Two packages are involved:

```bash
dotnet add package Allegory.Axiom.Localization.Abstractions
dotnet add package Allegory.Axiom.Localization
```

## Translation Files

Translation files are JSON files named after a culture code, placed inside one or more registered directories.

```
Resources/
  en.json
  en-US.json
  fr.json
  tr.json
```

Each file is a flat key-value map:

```json
{
  "greeting": "Hello!",
  "farewell": "Goodbye!",
  "welcome": "Welcome, {0}!"
}
```

String formatting uses `{0}`, `{1}`, etc. passed as arguments when indexing the localizer.

## Resource Registration

Resources are registered through `LocalizationOptions` in your [application package](./modularity/overview#application-packages). Each resource has a marker type (or name string), default culture and one or more translation directories.

```csharp
internal sealed class MyAppPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        // Register embedded file provider that contains your translation files
        builder.Services.Configure<FileProviderOptions>(options =>
        {
            options.AddEmbedded<MyAppPackage>();
        });

        // Register the localization resource
        builder.Services.Configure<LocalizationOptions>(options =>
        {
            options.Resources.Add<MyAppResource>(
                defaultCulture: "en",
                paths: "/Resources/Localization");
        });

        return ValueTask.CompletedTask;
    }
}
```

`MyAppResource` is a plain marker class used to identify the resource:

```csharp
public class MyAppResource { }
```

You can specify multiple directories. All translation files are combined into a single set. If the same culture has duplicate keys, the values from files in later directories take precedence and override earlier ones.

```csharp
options.Resources.Add<MyAppResource>("en",
    "/Resources/Localization/Base",
    "/Resources/Localization/Overrides");
```

::: info
For registering embedded resources correctly, follow the [Embedded Resources Setup](#embedded-resources-setup) section.
:::

## Resolving Strings

Inject `IStringLocalizer<T>` where `T` is your resource marker class:

```csharp
public class OrderService(IStringLocalizer<MyAppResource> localizer) : ITransientService
{
    public string GetGreeting(string name)
        => localizer["welcome", name].Value;
}
```

`IStringLocalizer<T>` is backed by `AxiomStringLocalizer` for registered resources. Unregistered types fall back to the standard [ResourceManagerStringLocalizer](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.localization.resourcemanagerstringlocalizer).

## Culture Fallback

When a key is not found for the current culture, Axiom walks up the culture hierarchy until the key is found or the default culture is reached.

Given default culture `en` and current culture `tr-TR`:

```
tr-TR.json → tr.json → en.json (default)
```

If the key exists in `tr.json` but not `tr-TR.json`, the `tr.json` value is returned. If not found in any Turkish culture file, the `en.json` value is used. If not found anywhere, the key itself is returned and `ResourceNotFound` is `true`.

```csharp
CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");

var result = localizer["greeting"];
// Checks: tr-TR.json → tr.json → en.json (default)
```

## Formatted Strings

Pass arguments after the key:

```csharp
// en.json: { "welcome": "Welcome, {0}!" }

var result = localizer["welcome", "Alice"];
// result.Value → "Welcome, Alice!"
```

## GetAllStrings

Retrieve all strings for the current culture, optionally including parent cultures:

```csharp
// Current culture only
var strings = localizer.GetAllStrings(includeParentCultures: false);

// Current culture + all parent cultures up to default
// Keys found in child cultures take priority over parents
var allStrings = localizer.GetAllStrings(includeParentCultures: true);
```

When `includeParentCultures: true`, the hierarchy is walked from specific to general. The first occurrence of each key wins no duplicates.

## Multiple Directories

Split translations across multiple directories and compose them per resource:

```
/Localization/
  Base/
    en.json     ← shared strings
    tr.json
  Module/
    en.json     ← module-specific strings, override base if same key
    tr.json
```

```csharp
options.Resources.Add<MyAppResource>("en", "/Localization/Base", "/Localization/Module");
```

Directories are processed in order. Later directories take precedence for duplicate keys within the same culture file.

You can also add paths to an existing resource after initial registration:

```csharp
options.Resources.Get<MyAppResource>().AddPaths("/Localization/Plugin");
```

## Dynamic Translations

`IAxiomStringLocalizer` exposes `Translations`, a `ConcurrentDictionary<string, ConcurrentDictionary<string, string>>` keyed by culture name. You can add or override translations at runtime:

```csharp
var localizer = (IAxiomStringLocalizer) factory.Create(typeof(MyAppResource));

localizer.Translations["en"]["dynamic-key"] = "Dynamic value";
```

All instances backed by the same resource share the same `Translations` dictionary a change in one is visible in all.

## Embedded Resources Setup

When serving translation files from embedded resources, configure your `.csproj` to generate the manifest and embed the files:

```xml
<PropertyGroup>
    <GenerateEmbeddedFilesManifest>true</GenerateEmbeddedFilesManifest>
</PropertyGroup>

<ItemGroup>
    <PackageReference Include="Microsoft.Extensions.FileProviders.Embedded" Version="10.0.0" />
</ItemGroup>

<ItemGroup>
    <None Remove="Resources\Localization\**" />
    <EmbeddedResource Include="Resources\Localization\**" />
</ItemGroup>
```

Then register the embedded provider in `FileProviderOptions`:

```csharp
builder.Services.Configure<FileProviderOptions>(options =>
{
    options.AddEmbedded<MyAppPackage>();
});
```
See [File Providers](./file-providers) for full provider configuration options.

::: warning
Be careful with file provider ordering. If any registered file provider (embedded, physical, or custom) has files from the same path as your localization directories, it will **completely replace** those translation files rather than merge with them. This is different from passing multiple paths to `options.Resources.Add`, which merges translations at the key level. To safely compose translations from multiple sources, use [multiple localization paths](#multiple-directories) instead of multiple file providers pointing at the same directory.
:::
