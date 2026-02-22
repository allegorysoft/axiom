using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Hosting;

public class AxiomHostApplicationBuilderTests
{
    [Fact]
    public async ValueTask ShouldBuildApplication()
    {
        var assembly = typeof(AxiomHostApplicationBuilderTests).Assembly;
        var builder = Host.CreateApplicationBuilder();
        var applicationBuilder = new AxiomHostApplicationBuilder();

        var application = await applicationBuilder.BuildAsync(builder, assembly);

        application.StartupAssembly.ShouldBe(assembly);
        application.Assemblies.ShouldContain(assembly);
        application.Assemblies.ShouldContain(typeof(Assembly1.Assembly1Package).Assembly);
        application.Assemblies.ShouldContain(typeof(Assembly2.Assembly2Package).Assembly);
        application.Assemblies.ShouldContain(typeof(Assembly3.Assembly3Package).Assembly);
    }
}