using System;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting.Plugins;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Hosting;

public class AxiomApplicationBuilderTests
{
    [Fact]
    public async ValueTask ShouldBuildApplication()
    {
        var assembly = typeof(AxiomApplicationBuilderTests).Assembly;
        var builder = Host.CreateApplicationBuilder();
        var applicationBuilder = new AxiomApplicationBuilder();

        var application = await applicationBuilder.BuildAsync(
            new AxiomApplicationBuilderContext(
                builder,
                assembly,
                new AssemblyDependencyRegistrar(builder.Services),
                []));

        application.Id.ShouldNotBe(Guid.Empty);
        application.StartupAssembly.ShouldBe(assembly);
        builder.Services.ShouldContain(s => s.ServiceType == typeof(AxiomApplication));

        application.Assemblies.ShouldContain(assembly);
        application.Assemblies.ShouldContain(typeof(Assembly1.Assembly1Package).Assembly);
        application.Assemblies.ShouldContain(typeof(Assembly2.Assembly2Package).Assembly);
        application.Assemblies.ShouldContain(typeof(Assembly3.Assembly3Package).Assembly);
    }

    [Fact]
    public async ValueTask ShouldLoadPlugins()
    {
        var assembly = typeof(Assembly1.Assembly1Package).Assembly;
        var builder = Host.CreateApplicationBuilder();
        var applicationBuilder = new AxiomApplicationBuilder();

        var application = await applicationBuilder.BuildAsync(
            new AxiomApplicationBuilderContext(
                builder,
                assembly,
                new AssemblyDependencyRegistrar(builder.Services),
                [new AxiomApplicationAssemblyPlugin(typeof(Assembly2.Assembly2Package).Assembly)]));

        application.StartupAssembly.ShouldBe(assembly);
        application.Assemblies.ShouldContain(assembly);
        application.Assemblies.ShouldContain(typeof(Assembly2.Assembly2Package).Assembly);
        application.Assemblies.ShouldNotContain(typeof(Assembly3.Assembly3Package).Assembly);
    }
}