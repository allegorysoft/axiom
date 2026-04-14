using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Allegory.Axiom.FileProviders;

public class FileProviderManager(IOptions<FileProviderOptions> options) : IFileProvider, ISingletonService
{
    public IFileProvider FileProvider { get; } = new CompositeFileProvider(options.Value.Providers);

    public IFileInfo GetFileInfo(string subpath) => FileProvider.GetFileInfo(subpath);
    public IDirectoryContents GetDirectoryContents(string subpath) => FileProvider.GetDirectoryContents(subpath);
    public IChangeToken Watch(string filter) => FileProvider.Watch(filter);
}