using System.Collections.Concurrent;

namespace Allegory.Axiom.Localization;

public interface ILocalizationStore
{
    ConcurrentDictionary<string, LocalizationContext> Contexts { get; }
}