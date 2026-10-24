using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace Allegory.Axiom.Localization;

public class AxiomStringLocalizer(LocalizationContext context) : IStringLocalizer
{
    protected LocalizationContext Context { get; } = context;

    public virtual LocalizedString this[string name]
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
            return Context.Translations.TryGetValue(culture.Name, out var translations)
                ? translations.Select(t => new LocalizedString(t.Key, t.Value))
                : [];
        }

        var cultures = GetCultureHierarchy(culture);
        var merged = new Dictionary<string, string>();

        foreach (var item in cultures)
        {
            if (!Context.Translations.TryGetValue(item, out var translations))
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
                if (!cultures.Contains(Context.DefaultCulture))
                {
                    cultures.Add(Context.DefaultCulture);
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
            if (Context.Translations.TryGetValue(current.Name, out var translations) &&
                translations.TryGetValue(name, out var value))
            {
                return value;
            }

            if (current.Parent.Equals(CultureInfo.InvariantCulture))
            {
                return current.Name.Equals(Context.DefaultCulture)
                    ? null
                    : Context.Translations
                        .GetValueOrDefault(Context.DefaultCulture)
                        ?.GetValueOrDefault(name) ?? null;
            }

            current = current.Parent;
        }
    }
}