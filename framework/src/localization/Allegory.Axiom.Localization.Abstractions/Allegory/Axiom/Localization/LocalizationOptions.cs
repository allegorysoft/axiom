using System.Collections.Generic;

namespace Allegory.Axiom.Localization;

public class LocalizationOptions
{
    public HashSet<LocalizationResourceOptions> Resources { get; } = [];
    public Dictionary<string, string> ExceptionCodeMappings { get; } = new();
}