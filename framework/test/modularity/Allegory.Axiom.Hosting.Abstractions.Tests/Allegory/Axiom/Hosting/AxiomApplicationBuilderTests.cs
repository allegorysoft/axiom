using System;
using System.Reflection;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting.Plugins;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Hosting;

public class AxiomApplicationBuilderTests
{
    protected Assembly Assembly { get; } = typeof(AxiomApplicationBuilderTests).Assembly;
    protected IHostApplicationBuilder Builder { get; } = Host.CreateApplicationBuilder();
    protected AxiomApplicationBuilder ApplicationBuilder { get; } = new();

    [Fact]
    public async ValueTask ShouldBuildApplication()
    {
        var application = await ApplicationBuilder.BuildAsync(
            new AxiomApplicationBuilderContext(
                Builder,
                Assembly,
                new AssemblyDependencyRegistrar(Builder.Services),
                []));

        application.Id.ShouldNotBe(Guid.Empty);
        application.StartupAssembly.ShouldBe(Assembly);
        Builder.Services.ShouldContain(s => s.ServiceType == typeof(AxiomApplication));
    }

    [Fact]
    public async ValueTask ShouldInvokeConfigureMethods()
    {
        var postConfigureAction = false;
        Builder.Services.AddPostConfigureAction(_ => postConfigureAction = true);

        var application = await ApplicationBuilder.BuildAsync(
            new AxiomApplicationBuilderContext(
                Builder,
                Assembly,
                new AssemblyDependencyRegistrar(Builder.Services),
                []));

        HostingAbstractionsTestsPackage.ConfigureApplication.ShouldBeTrue();
        HostingAbstractionsTestsPackage.PostConfigureApplication.ShouldBeTrue();
        postConfigureAction.ShouldBeTrue();
    }

    [Fact]
    public async ValueTask ShouldDiscoverDependenciesAndRegisterServices()
    {
        var application = await ApplicationBuilder.BuildAsync(
            new AxiomApplicationBuilderContext(
                Builder,
                Assembly,
                new AssemblyDependencyRegistrar(Builder.Services),
                []));

        application.Assemblies.ShouldContain(typeof(AxiomApplicationBuilderTests).Assembly);
        application.Assemblies.ShouldContain(typeof(Assembly1.Assembly1Package).Assembly);
        application.Assemblies.ShouldContain(typeof(Assembly2.Assembly2Package).Assembly);
        application.Assemblies.ShouldContain(typeof(Assembly3.Assembly3Package).Assembly);

        Builder.Services.ShouldContain(s => s.ServiceType == typeof(TestService));
    }

    [Fact]
    public async ValueTask ShouldLoadPlugins()
    {
        var assembly = typeof(Assembly1.Assembly1Package).Assembly;

        var application = await ApplicationBuilder.BuildAsync(
            new AxiomApplicationBuilderContext(
                Builder,
                assembly,
                new AssemblyDependencyRegistrar(Builder.Services),
                [new AxiomApplicationAssemblyPlugin(typeof(Assembly2.Assembly2Package).Assembly)]));

        application.StartupAssembly.ShouldBe(assembly);
        application.Assemblies.ShouldContain(assembly);
        application.Assemblies.ShouldContain(typeof(Assembly2.Assembly2Package).Assembly);
        application.Assemblies.ShouldNotContain(typeof(Assembly3.Assembly3Package).Assembly);
    }
}

file class TestService : ITransientService {}