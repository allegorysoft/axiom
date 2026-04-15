using System;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.FileProviders;

public class FileProviderOptionsTests
{
    [Fact]
    public void ShouldAddEmbeddedFileProviderWithGenericType()
    {
        var options = new FileProviderOptions();

        options.AddEmbedded<FileProviderOptionsTests>();

        options.Providers.ShouldHaveSingleItem();
        options.Providers[0].ShouldBeOfType<EmbeddedFileProvider>();
    }

    [Fact]
    public void ShouldAddEmbeddedFileProviderWithAssembly()
    {
        var options = new FileProviderOptions();

        options.AddEmbedded(Assembly.GetExecutingAssembly());

        options.Providers.ShouldHaveSingleItem();
        options.Providers[0].ShouldBeOfType<EmbeddedFileProvider>();
    }

    [Fact]
    public void ShouldAddPhysicalFileProvider()
    {
        var options = new FileProviderOptions();

        options.AddPhysical(AppContext.BaseDirectory);

        options.Providers.ShouldHaveSingleItem();
        options.Providers[0].ShouldBeOfType<PhysicalFileProvider>();
    }
}