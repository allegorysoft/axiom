using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.FileProvider;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Localization;

public class LocalizationProvider : ILocalizationProvider, ISingletonService
{
    public LocalizationProvider(IOptions<LocalizationOptions> options, IFileProviderManager fileProviderManager)
    {
        Options = options;
        FileProviderManager = fileProviderManager;

        FillContexts();
    }

    protected IOptions<LocalizationOptions> Options { get; }
    protected IFileProviderManager FileProviderManager { get; }

    public ConcurrentDictionary<string, LocalizationContext> Contexts { get; } = [];

    protected void FillContexts()
    {
        foreach (var resource in Options.Value.Resources)
        {
            FillResource(resource);
        }
    }

    protected void FillResource(LocalizationResourceOptions resource)
    {
        var cultures = new Dictionary<string, Dictionary<string, string>>();

        foreach (var path in resource.Paths)
        {
            foreach (var file in FileProviderManager.GetDirectoryContents(path))
            {
                if (!string.Equals(Path.GetExtension(file.Name), ".json", StringComparison.OrdinalIgnoreCase) ||
                    file.IsDirectory)
                {
                    continue;
                }

                FillCulture(file, cultures);
            }
        }

        var context = new LocalizationContext
        {
            DefaultCulture = resource.DefaultCulture.Name,
            Translations = cultures.ToFrozenDictionary(
                x => x.Key,
                x => x.Value.ToFrozenDictionary())
        };

        Contexts[resource.Name] = context;
    }

    protected void FillCulture(IFileInfo file, Dictionary<string, Dictionary<string, string>> cultures)
    {
        var culture = Path.GetFileNameWithoutExtension(file.Name);

        using var stream = file.CreateReadStream();

        var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);

        if (translations is null)
        {
            return;
        }

        if (!cultures.TryGetValue(culture, out var existing))
        {
            cultures[culture] = translations;
        }
        else
        {
            foreach (var (key, value) in translations)
            {
                existing[key] = value;
            }
        }
    }
}