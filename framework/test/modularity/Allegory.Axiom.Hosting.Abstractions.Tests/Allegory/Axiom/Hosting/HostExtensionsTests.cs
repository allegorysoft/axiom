using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Hosting;

public class HostExtensionsTests
{
    protected HostApplicationBuilder Builder { get; } = Host.CreateApplicationBuilder();

    [Fact]
    public async ValueTask ShouldConfigureApplication()
    {
        var postConfigureAction = false;
        Builder.Services.AddPostConfigureAction(_ => postConfigureAction = true);
        Builder.Services.ShouldNotContain(t => t.ServiceType == typeof(AxiomApplication));

        var application = await Builder.ConfigureApplicationAsync();

        application.Id.ShouldNotBe(Guid.Empty);
        Builder.Services.ShouldContain(t => t.ServiceType == typeof(AxiomApplication));
        postConfigureAction.ShouldBeTrue();
    }

    [Fact]
    public async ValueTask ShouldInitializeApplication()
    {
        await Builder.ConfigureApplicationAsync();
        var host = Builder.Build();
        await host.InitializeApplicationAsync();

        AxiomHostingAbstractionsTestsPackage.InitializeApplication.ShouldBeTrue();
    }

    [Fact]
    public async ValueTask ShouldSetEntryAssemblyWhenStartupAssemblyIsNull()
    {
        var application = await Builder.ConfigureApplicationAsync();

        application.StartupAssembly.ShouldBe(Assembly.GetEntryAssembly());
    }

    [Fact]
    public async ValueTask ShouldOverrideDependencyRegistrar()
    {
        await Builder.ConfigureApplicationAsync(o => o.DependencyRegistrar = new CustomDependencyRegistrar(Builder.Services));

        Builder.Services.ShouldContain(t => t.ServiceType == typeof(SomeClassRegisterMe));
    }

    [Fact]
    public async ValueTask ShouldOverrideApplicationBuilder()
    {
        var application = await Builder.ConfigureApplicationAsync(o => o.ApplicationBuilder = new CustomApplicationBuilder());

        application.Id.ShouldBe(Guid.Empty);
        application.Assemblies.Count.ShouldBe(0);
    }

    [Fact]
    public async ValueTask ShouldPassPluginsToBuilder()
    {
        var assembly = typeof(Assembly1.Assembly1Package).Assembly;
        var application = await Builder.ConfigureApplicationAsync(o =>
        {
            o.StartupAssembly = assembly;
            o.Plugins.Add(new AxiomApplicationAssemblyPlugin(typeof(Assembly2.Assembly2Package).Assembly));
        });

        application.StartupAssembly.ShouldBe(assembly);
        application.Assemblies.ShouldContain(assembly);
        application.Assemblies.ShouldContain(typeof(Assembly2.Assembly2Package).Assembly);
        application.Assemblies.ShouldNotContain(typeof(Assembly3.Assembly3Package).Assembly);
    }
}

file class CustomDependencyRegistrar(IServiceCollection serviceCollection) :
    AssemblyDependencyRegistrar(serviceCollection)
{
    protected override void RegisterImplementation(Type implementation)
    {
        ServiceCollection.AddTransient(implementation);
    }

    protected override IEnumerable<Type> GetImplementationTypes(Assembly assembly)
    {
        return assembly.GetTypes().Where(t => t.IsClass && t.Name.EndsWith("RegisterMe"));
    }
}

internal class SomeClassRegisterMe {}

file class CustomApplicationBuilder : AxiomApplicationBuilder
{
    protected override ValueTask<AxiomApplication> BuildAsync()
    {
        return ValueTask.FromResult(new AxiomApplication(Guid.Empty, Context.StartupAssembly, []));
    }
}