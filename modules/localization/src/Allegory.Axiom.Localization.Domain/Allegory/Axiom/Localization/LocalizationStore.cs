using System.Collections.Concurrent;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.Localization;

public class LocalizationStore : ILocalizationStore, ISingletonService
{
    public ConcurrentDictionary<string, LocalizationContext> Contexts { get; } = [];
}