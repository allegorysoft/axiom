using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Allegory.Axiom.FileProviders;
using Microsoft.Extensions.Localization;

namespace Allegory.Axiom.Localization;

public class AxiomStringLocalizer : IAxiomStringLocalizer
{
    public AxiomStringLocalizer(LocalizationResourceOptions options, FileProviderManager fileProviderManager)
    {
        Options = options;
        FileProviderManager = fileProviderManager;
        InitialSeed();
    }

    public LocalizationResourceOptions Options { get; }
    public ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Translations { get; } = new(StringComparer.OrdinalIgnoreCase);
    public FileProviderManager FileProviderManager { get; }

    public LocalizedString this[string name]
    {
        get
        {
            var value = GetTranslation(name);
            return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
        }
    }

    public virtual LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var translation = GetTranslation(name);
            var value = string.Format(CultureInfo.CurrentCulture, translation ?? name, arguments);
            return new LocalizedString(name, value, resourceNotFound: translation == null);
        }
    }

    public virtual IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var culture = CultureInfo.CurrentUICulture;

        if (!includeParentCultures)
        {
            return Translations.TryGetValue(culture.Name, out var translations)
                ? translations.Select(t => new LocalizedString(t.Key, t.Value))
                : [];
        }

        var cultures = GetCultureHierarchy(culture);
        var merged = new Dictionary<string, string>();

        foreach (var item in cultures)
        {
            if (!Translations.TryGetValue(item, out var translations))
            {
                continue;
            }

            foreach (var (key, value) in translations)
            {
                merged.TryAdd(key, value);
            }
        }

        return merged.Select(t => new LocalizedString(t.Key, t.Value));
    }

    protected virtual List<string> GetCultureHierarchy(CultureInfo culture)
    {
        var cultures = new List<string>();
        var current = culture;
        while (true)
        {
            cultures.Add(current.Name);

            if (current.Parent.Equals(CultureInfo.InvariantCulture))
            {
                if (!cultures.Contains(Options.DefaultCulture.Name))
                {
                    cultures.Add(Options.DefaultCulture.Name);
                }

                break;
            }

            current = current.Parent;
        }

        return cultures;
    }

    protected virtual string? GetTranslation(string name, CultureInfo? culture = null)
    {
        var current = culture ?? CultureInfo.CurrentUICulture;

        while (true)
        {
            if (Translations.TryGetValue(current.Name, out var translations) &&
                translations.TryGetValue(name, out var value))
            {
                return value;
            }

            if (current.Parent.Equals(CultureInfo.InvariantCulture))
            {
                return current.Equals(Options.DefaultCulture)
                    ? null
                    : Translations[Options.DefaultCulture.Name].GetValueOrDefault(name);
            }

            current = current.Parent;
        }
    }

    private void InitialSeed()
    {
        foreach (var path in Options.Paths)
        {
            foreach (var content in FileProviderManager.GetDirectoryContents(path).Where(x => !x.IsDirectory))
            {
                var key = new CultureInfo(Path.GetFileNameWithoutExtension(content.Name));
                var translations = Translations.GetOrAdd(key.Name, _ => new ConcurrentDictionary<string, string>());
                using var stream = content.CreateReadStream();
                var contentTranslations = JsonSerializer.Deserialize<Dictionary<string, string>>(stream) ?? [];

                foreach (var text in contentTranslations)
                {
                    translations[text.Key] = text.Value;
                }
            }
        }
    }
}