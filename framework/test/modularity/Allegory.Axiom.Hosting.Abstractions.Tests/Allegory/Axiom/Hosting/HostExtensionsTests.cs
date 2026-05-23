using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Hosting;

public class HostExtensionsTests
{
    protected HostApplicationBuilder Builder { get; } = Host.CreateApplicationBuilder();

    [Fact]
    public async Task ShouldInitializeApplication()
    {
        await Builder.ConfigureApplicationAsync();
        var host = Builder.Build();
        await host.InitializeApplicationAsync();

        HostingAbstractionsTestsPackage.InitializeApplication.ShouldBeTrue();
    }
}