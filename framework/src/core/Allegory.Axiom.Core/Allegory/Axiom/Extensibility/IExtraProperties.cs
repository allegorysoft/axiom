using System.Collections.Generic;

namespace Allegory.Axiom.Extensibility;

public interface IExtraProperties
{
    IDictionary<string, object?> ExtraProperties { get; }
}