using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.FileProvider;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Localization;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class InProcessLocalizationStore : ILocalizationStore, ISingletonService
{
    public InProcessLocalizationStore(
        IOptions<LocalizationOptions> options,
        IFileProviderManager fileProviderManager)
    {
        FillContexts(options, fileProviderManager);
    }

    public ConcurrentDictionary<string, LocalizationContext> Contexts { get; } = [];

    protected void FillContexts(IOptions<LocalizationOptions> options, IFileProviderManager fileProviderManager)
    {
        foreach (var resource in options.Value.Resources)
        {
            var context = Contexts.GetOrAdd(resource.Name, static _ => new LocalizationContext());
            context.DefaultCulture = resource.DefaultCulture.Name;

            var cultures = new Dictionary<string, Dictionary<string, string>>();

            foreach (var path in resource.Paths)
            {
                foreach (var content in fileProviderManager.GetDirectoryContents(path).Where(x => !x.IsDirectory))
                {
                    var culture = Path.GetFileNameWithoutExtension(content.Name);
                    if (!cultures.TryGetValue(culture, out var translations))
                    {
                        cultures[culture] = translations = new Dictionary<string, string>();
                    }

                    using var stream = content.CreateReadStream();
                    var contentTranslations = JsonSerializer.Deserialize<Dictionary<string, string>>(stream) ?? [];

                    foreach (var text in contentTranslations)
                    {
                        translations[text.Key] = text.Value;
                    }
                }
            }

            context.Translations = cultures.ToFrozenDictionary(
                k => k.Key,
                v => v.Value.ToFrozenDictionary());
        }
    }
}