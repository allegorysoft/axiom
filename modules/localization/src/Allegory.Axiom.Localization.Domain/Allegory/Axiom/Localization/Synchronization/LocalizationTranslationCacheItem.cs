using System.Collections.Frozen;
using System.ComponentModel;

namespace Allegory.Axiom.Localization;

[ImmutableObject(true)]
public sealed class LocalizationTranslationCacheItem
{
    public FrozenDictionary<string, string> Translations { get; set; } = null!;
}