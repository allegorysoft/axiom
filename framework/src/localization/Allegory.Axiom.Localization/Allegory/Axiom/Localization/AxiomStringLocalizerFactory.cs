using System;
using System.Linq;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Localization;

public class AxiomStringLocalizerFactory(
    IServiceProvider serviceProvider,
    IOptions<LocalizationOptions> options)
    : IStringLocalizerFactory, ISingletonService
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public LocalizationOptions Options { get; } = options.Value;
    
    //Cache localizers
    
    public virtual IStringLocalizer Create(Type resourceType)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceType.FullName);
        return Create(resourceType.FullName);
    }

    public virtual IStringLocalizer Create(string baseName, string location)
    {
        return Create(baseName);
    }

    protected virtual IAxiomStringLocalizer Create(string resource)
    {
        var resourceOptions = Options.Resources.FirstOrDefault(x => x.Resource == resource);

        if (resourceOptions == null)
        {
            // return ResourceManagerStringLocalizer
            throw new NotImplementedException();
        }

        return ActivatorUtilities.CreateInstance<AxiomStringLocalizer>(ServiceProvider, resourceOptions);
    }
}