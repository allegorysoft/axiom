using System;
using System.Linq;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.FileProviders;

public class FileProviderManagerTests
{
    public FileProviderManagerTests()
    {
        var collection = new ServiceCollection();
        var registrar = new AssemblyDependencyRegistrar(collection);
        registrar.Register(typeof(FileProviderManager).Assembly);

        collection.Configure<FileProviderOptions>(o =>
        {
            o.AddEmbedded<FileProviderManagerTests>();
            o.AddPhysical(AppContext.BaseDirectory);
        });

        var provider = collection.BuildServiceProvider();
        Manager = provider.GetRequiredService<FileProviderManager>();
    }

    protected FileProviderManager Manager { get; set; }

    [Fact]
    public void ShouldReverseProvidersOrder()
    {
        var compositeProvider = Manager.FileProvider;

        var providers = compositeProvider.FileProviders.ToList();

        providers.Count.ShouldBe(2);
        providers[0].ShouldBeOfType<PhysicalFileProvider>();// Added last, so it should be first
        providers[1].ShouldBeOfType<EmbeddedFileProvider>();// Added first, so it should be last
    }

    [Fact]
    public void ShouldGetFileInfo()
    {
        var fileInfo = Manager.GetFileInfo("Allegory.Axiom.FileProviders.Tests.dll");

        fileInfo.ShouldNotBeNull();
        fileInfo.Exists.ShouldBeTrue();
        fileInfo.Name.ShouldBe("Allegory.Axiom.FileProviders.Tests.dll");
    }

    [Fact]
    public void ShouldGetDirectoryContents()
    {
        var directoryContents = Manager.GetDirectoryContents("/");

        directoryContents.ShouldNotBeNull();
        directoryContents.Exists.ShouldBeTrue();
    }

    [Fact]
    public void ShouldWatchFiles()
    {
        var changeToken = Manager.Watch("*.json");

        changeToken.ShouldNotBeNull();
        changeToken.ActiveChangeCallbacks.ShouldBeTrue();
    }
}