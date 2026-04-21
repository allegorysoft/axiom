using System.Collections.Concurrent;
using Microsoft.Extensions.Localization;

namespace Allegory.Axiom.Localization;

public interface IAxiomStringLocalizer : IStringLocalizer
{
    LocalizationResourceOptions Options { get; }
    ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Translations { get; }
}