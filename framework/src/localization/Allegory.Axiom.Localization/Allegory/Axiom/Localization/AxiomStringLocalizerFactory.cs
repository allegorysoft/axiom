using System;
using System.Collections.Concurrent;
using System.Linq;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Localization;

public class AxiomStringLocalizerFactory(
    IServiceProvider serviceProvider,
    IOptions<LocalizationOptions> options,
    ResourceManagerStringLocalizerFactory localizerFactory)
    : IStringLocalizerFactory, ISingletonService
{
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;
    protected LocalizationOptions Options { get; } = options.Value;
    protected ResourceManagerStringLocalizerFactory LocalizerFactory { get; } = localizerFactory;
    protected internal ConcurrentDictionary<string, IStringLocalizer> LocalizerCache { get; } = new();

    public virtual IStringLocalizer Create(Type resourceType)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceType.FullName);

        if (LocalizerCache.TryGetValue(resourceType.FullName, out var localizer))
        {
            return localizer;
        }

        var options = Options.Resources.FirstOrDefault(o => o.Name == resourceType.FullName);
        if (options == null)
        {
            return LocalizerFactory.Create(resourceType);
        }

        localizer = ActivatorUtilities.CreateInstance<AxiomStringLocalizer>(ServiceProvider, options);
        LocalizerCache[resourceType.FullName] = localizer;
        return localizer;
    }

    public virtual IStringLocalizer Create(string baseName, string location)
    {
        if (LocalizerCache.TryGetValue(baseName, out var localizer))
        {
            return localizer;
        }

        var options = Options.Resources.FirstOrDefault(o => o.Name == baseName);
        if (options == null)
        {
            return LocalizerFactory.Create(baseName, location);
        }

        localizer = ActivatorUtilities.CreateInstance<AxiomStringLocalizer>(ServiceProvider, options);
        LocalizerCache[baseName] = localizer;
        return localizer;
    }
}