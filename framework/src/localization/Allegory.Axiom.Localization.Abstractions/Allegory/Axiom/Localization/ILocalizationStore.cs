using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace Allegory.Axiom.Localization;

public interface ILocalizationStore
{
    ConcurrentDictionary<string, LocalizationContext> Contexts { get; }
}

// Cache models:
// localizations:resources = [ { Name=XModule, DefaultCulture=en } ]
// localizations:translations:xmodule = { "en": { "key": value }, "tr": { "key": value } }

public class LocalizationContext
{
    public string DefaultCulture { get; set; } = null!;
    public FrozenDictionary<string, FrozenDictionary<string, string>> Translations { get; set; } = null!;
}