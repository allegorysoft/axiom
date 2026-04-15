using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Allegory.Axiom.FileProviders;

public class FileProviderManager : IFileProvider, ISingletonService
{
    public FileProviderManager(IOptions<FileProviderOptions> options)
    {
        options.Value.Providers.Reverse();
        FileProvider = new CompositeFileProvider(options.Value.Providers);
    }

    public CompositeFileProvider FileProvider { get; protected set; }

    public virtual IFileInfo GetFileInfo(string subpath) => FileProvider.GetFileInfo(subpath);
    public virtual IDirectoryContents GetDirectoryContents(string subpath) => FileProvider.GetDirectoryContents(subpath);
    public virtual IChangeToken Watch(string filter) => FileProvider.Watch(filter);
}