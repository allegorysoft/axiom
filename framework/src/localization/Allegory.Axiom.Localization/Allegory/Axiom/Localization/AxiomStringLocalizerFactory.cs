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
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public LocalizationOptions Options { get; } = options.Value;
    public ResourceManagerStringLocalizerFactory LocalizerFactory { get; } = localizerFactory;
    protected internal ConcurrentDictionary<string, IStringLocalizer> LocalizerCache { get; } = new();

    public virtual IStringLocalizer Create(Type resourceType)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceType.FullName);

        var options = Options.Resources.FirstOrDefault(o => o.Resource == resourceType.FullName);
        if (options == null)
        {
            return LocalizerFactory.Create(resourceType);
        }

        return LocalizerCache.GetOrAdd(
            options.Resource,
            _ => ActivatorUtilities.CreateInstance<AxiomStringLocalizer>(ServiceProvider, options));
    }

    public virtual IStringLocalizer Create(string baseName, string location)
    {
        var options = Options.Resources.FirstOrDefault(o => o.Resource == baseName);
        if (options == null)
        {
            return LocalizerFactory.Create(baseName, location);
        }

        return LocalizerCache.GetOrAdd(
            options.Resource,
            _ => ActivatorUtilities.CreateInstance<AxiomStringLocalizer>(ServiceProvider, options));
    }
}