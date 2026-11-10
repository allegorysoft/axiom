using System.Collections.Immutable;

namespace Allegory.Axiom.Localization;

public class LocalizationResourceCacheItem
{
    /*
     localizations:resources
       [
            {
              "name": "App",
              "defaultCulture": "en",
              "cultures": ["en", "tr", "fr", "de"]
            },
            {
              "name": "Identity",
              "defaultCulture": "en",
              "cultures": ["en", "tr"]
            }
        ]
     */
    public string Name { get; set; } = null!;
    public string DefaultCulture { get; set; } = null!;
    public ImmutableArray<string> Cultures { get; set; }
}