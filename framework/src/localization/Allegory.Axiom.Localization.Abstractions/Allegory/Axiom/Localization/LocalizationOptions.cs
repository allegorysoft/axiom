using System.Collections.Generic;

namespace Allegory.Axiom.Localization;

public class LocalizationOptions
{
    public HashSet<LocalizationResourceOptions> Resources { get; } = [];
}