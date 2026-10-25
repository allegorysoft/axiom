using System.Collections.Concurrent;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.Localization;

public class LocalizationStore : ILocalizationStore, ISingletonService
{
    public ConcurrentDictionary<string, LocalizationContext> Contexts { get; } = [];
    
    
// Cache models:
// localizations:resources = [ { Name=XModule, DefaultCulture=en } ]
// localizations:translations:xmodule = { "en": { "key": value }, "tr": { "key": value } }

}