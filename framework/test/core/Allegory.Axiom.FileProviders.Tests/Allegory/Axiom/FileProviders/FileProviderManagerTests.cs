using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.FileProviders;

public class FileProviderManagerTests : LightIntegrationTestBase
{
    protected FileProviderManager Manager => Service<FileProviderManager>();

    protected override ValueTask ConfigureAsync(
        IServiceCollection services,
        AssemblyDependencyRegistrar registrar)
    {
        registrar.Register(typeof(FileProviderManager).Assembly);

        services.Configure<FileProviderOptions>(o =>
        {
            o.AddEmbedded<FileProviderManagerTests>();
            o.AddPhysical(AppContext.BaseDirectory);
        });

        return ValueTask.CompletedTask;
    }

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