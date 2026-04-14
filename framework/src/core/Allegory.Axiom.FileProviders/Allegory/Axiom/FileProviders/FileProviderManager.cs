using System;
using System.Linq;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Allegory.Axiom.FileProviders;

public class FileProviderManager : IFileProvider, ISingletonService
{
    private readonly Lazy<CompositeFileProvider> _lazyFileProvider;

    public FileProviderManager(
        IOptions<FileProviderOptions> options,
        IHostEnvironment hostEnvironment)
    {
        Options = options.Value;
        HostEnvironment = hostEnvironment;

        _lazyFileProvider = new Lazy<CompositeFileProvider>(CreateFileProvider);
    }

    protected FileProviderOptions Options { get; }
    protected IHostEnvironment HostEnvironment { get; }
    public CompositeFileProvider FileProvider => _lazyFileProvider.Value;

    public IFileInfo GetFileInfo(string subpath) => FileProvider.GetFileInfo(subpath);
    public IDirectoryContents GetDirectoryContents(string subpath) => FileProvider.GetDirectoryContents(subpath);
    public IChangeToken Watch(string filter) => FileProvider.Watch(filter);

    protected virtual CompositeFileProvider CreateFileProvider()
    {
        var providers = Options.Providers;

        if (Options.AddContentRootFileProvider)
        {
            providers.Add(HostEnvironment.ContentRootFileProvider);
        }

        providers.AddRange(
            Options.PhysicalPaths.Select(path => new PhysicalFileProvider(path)));

        providers.Reverse();

        return new CompositeFileProvider(providers);
    }
}