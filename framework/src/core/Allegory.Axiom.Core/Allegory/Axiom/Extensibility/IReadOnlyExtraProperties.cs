using System.Collections.Generic;

namespace Allegory.Axiom.Extensibility;

public interface IReadOnlyExtraProperties
{
    IReadOnlyDictionary<string, object?> ExtraProperties { get; }
}