using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.FileProvider;

public class FileProviderManagerTests(FileProviderManagerFixture fixture)
    : IClassFixture<FileProviderManagerFixture>
{
    protected IFileProviderManager Manager { get; } = fixture.Service<IFileProviderManager>();

    [Fact]
    public async Task ShouldReverseProvidersOrder()
    {
        var service = await fixture.CreateServiceProviderAsync(postConfigure: builder =>
        {
            builder.Services.Configure<FileProviderOptions>(o =>
            {
                o.Providers.Clear();
                o.AddEmbedded<FileProviderManagerTests>();
                o.AddPhysical(AppContext.BaseDirectory);
            });
        });

        var provider = new FileProviderManager(service.GetRequiredService<IOptions<FileProviderOptions>>());

        var providers = provider.FileProvider.FileProviders.ToList();

        providers.Count.ShouldBe(2);
        providers[0].ShouldBeOfType<PhysicalFileProvider>();// Added last, so it should be first
        providers[1].ShouldBeOfType<ManifestEmbeddedFileProvider>();// Added first, so it should be last
    }

    [Fact]
    public void ShouldGetFileInfo()
    {
        var fileInfo = Manager.GetFileInfo("/Resources/NewFile1.txt");

        fileInfo.ShouldNotBeNull();
        fileInfo.Exists.ShouldBeTrue();
        fileInfo.Name.ShouldBe("NewFile1.txt");
    }

    [Fact]
    public void ShouldGetDirectoryContents()
    {
        var directoryContents = Manager.GetDirectoryContents("/Resources");

        directoryContents.ShouldNotBeNull();
        directoryContents.Exists.ShouldBeTrue();
        directoryContents.Count().ShouldBe(2);
    }

    [Fact]
    public void ShouldWatchFiles()
    {
        var changeToken = Manager.Watch("*.json");

        changeToken.ShouldNotBeNull();
        changeToken.ActiveChangeCallbacks.ShouldBeTrue();
    }
}

public class FileProviderManagerFixture : IntegrationTest
{
    protected override Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<FileProviderOptions>(o =>
        {
            o.AddEmbedded<FileProviderManagerTests>();
            o.AddPhysical(AppContext.BaseDirectory);
        });

        return Task.CompletedTask;
    }
}