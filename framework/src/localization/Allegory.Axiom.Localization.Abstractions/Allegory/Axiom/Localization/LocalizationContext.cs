using System.Collections.Frozen;

namespace Allegory.Axiom.Localization;

public class LocalizationContext
{
    public string DefaultCulture { get; set; } = null!;
    public FrozenDictionary<string, FrozenDictionary<string, string>> Translations { get; set; } = null!;
}