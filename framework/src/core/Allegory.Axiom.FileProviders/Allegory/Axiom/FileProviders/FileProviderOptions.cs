using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace Allegory.Axiom.FileProviders;

public class FileProviderOptions
{
    public List<IFileProvider> Providers { get; } = [];

    public void AddEmbedded<T>(string? root = null)
    {
        Providers.Add(new EmbeddedFileProvider(typeof(T).Assembly, root));
    }

    public void AddEmbedded(Assembly assembly, string? root = null)
    {
        Providers.Add(new EmbeddedFileProvider(assembly, root));
    }

    public void AddPhysical(string root, ExclusionFilters? filters = null)
    {
        Providers.Add(
            filters.HasValue
                ? new PhysicalFileProvider(root, filters.Value)
                : new PhysicalFileProvider(root));
    }
}