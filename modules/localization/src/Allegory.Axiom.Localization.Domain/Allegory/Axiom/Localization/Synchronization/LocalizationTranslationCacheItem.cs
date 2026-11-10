using System.Collections.Frozen;
using System.ComponentModel;
using Allegory.Axiom.Caching;

namespace Allegory.Axiom.Localization;

[ImmutableObject(true)]
[CacheName("localizations:translations")]
public sealed class LocalizationTranslationCacheItem
{
    /*
 localizations:translations:App:en
 {
     "Welcome": "Welcome",
     "Logout": "Log out",
     "Save": "Save",
     "Cancel": "Cancel"
 }
*/
    public FrozenDictionary<string, string> Translations { get; set; } = null!;
}