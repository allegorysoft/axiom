using System.Collections.Concurrent;
using Microsoft.Extensions.Localization;

namespace Allegory.Axiom.Localization;

public interface IAxiomStringLocalizer : IStringLocalizer
{
    ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Translations { get; }
}