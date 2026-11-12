using System.Collections.Generic;

namespace Allegory.Axiom.Localization;

public class LocalizationResourceProjection
{
    public string Name { get; set; } = null!;
    public string DefaultCulture { get; set; } = null!;
    public IReadOnlyList<string> Cultures { get; set; } = null!;
}