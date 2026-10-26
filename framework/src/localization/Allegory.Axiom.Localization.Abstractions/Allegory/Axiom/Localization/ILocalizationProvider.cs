using System.Collections.Concurrent;

namespace Allegory.Axiom.Localization;

public interface ILocalizationProvider
{
    ConcurrentDictionary<string, LocalizationContext> Contexts { get; }
}