using System;
using System.Collections.Concurrent;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Allegory.Axiom.Localization;

public class AxiomStringLocalizerFactory(
    IServiceProvider serviceProvider,
    ILocalizationStore store,
    ResourceManagerStringLocalizerFactory localizerFactory)
    : IStringLocalizerFactory, ISingletonService
{
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;
    protected ILocalizationStore Store { get; } = store;
    protected ResourceManagerStringLocalizerFactory LocalizerFactory { get; } = localizerFactory;
    protected internal ConcurrentDictionary<string, IStringLocalizer> LocalizerCache { get; } = new();

    public virtual IStringLocalizer Create(Type resourceType)
    {
        var resourceName = ResourceNameAttribute.Get(resourceType);

        if (LocalizerCache.TryGetValue(resourceName, out var localizer))
        {
            return localizer;
        }

        if (Store.Contexts.TryGetValue(resourceName, out var context))
        {
            return LocalizerCache.GetOrAdd(
                resourceName,
                _ => ActivatorUtilities.CreateInstance<AxiomStringLocalizer>(ServiceProvider, context));
        }

        return LocalizerFactory.Create(resourceType);
    }

    public virtual IStringLocalizer Create(string baseName, string location)
    {
        if (LocalizerCache.TryGetValue(baseName, out var localizer))
        {
            return localizer;
        }

        if (Store.Contexts.TryGetValue(baseName, out var context))
        {
            return LocalizerCache.GetOrAdd(
                baseName,
                _ => ActivatorUtilities.CreateInstance<AxiomStringLocalizer>(ServiceProvider, context));
        }

        return LocalizerFactory.Create(baseName, location);
    }
}