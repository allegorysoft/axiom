using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Testing.Platform.Services;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Hosting;

public class HostExtensionsTests
{
    [Fact]
    public async ValueTask ShouldConfigureApplication()
    {
        var builder = Host.CreateApplicationBuilder();
        await builder.ConfigureApplicationAsync();

        AxiomHostingAbstractionsTestsPackage.ConfigureApplication.ShouldBeTrue();
        AxiomHostingAbstractionsTestsPackage.PostConfigureApplication.ShouldBeTrue();
    }

    [Fact]
    public async ValueTask ShouldInitializeApplication()
    {
        var builder = Host.CreateApplicationBuilder();
        await builder.ConfigureApplicationAsync();
        var host = builder.Build();
        await host.InitializeApplicationAsync();

        AxiomHostingAbstractionsTestsPackage.InitializeApplication.ShouldBeTrue();
    }

    [Fact]
    public async ValueTask ShouldDiscoverAssembliesAndRegisterServices()
    {
        var builder = Host.CreateApplicationBuilder();
        await builder.ConfigureApplicationAsync();
        var host = builder.Build();
        await host.InitializeApplicationAsync();

        var application = ServiceProviderExtensions.GetRequiredService<AxiomApplication>(host.Services);
        application.Assemblies.ShouldContain(typeof(HostExtensionsTests).Assembly);
        application.Assemblies.ShouldContain(typeof(Assembly1.Assembly1Package).Assembly);
        application.Assemblies.ShouldContain(typeof(Assembly2.Assembly2Package).Assembly);
        application.Assemblies.ShouldContain(typeof(Assembly3.Assembly3Package).Assembly);

        var service = ServiceProviderExtensions.GetService<TestService>(host.Services);
        service.ShouldNotBeNull();
        service.GetNumber().ShouldBe(1);
    }

    [Fact]
    public async ValueTask ShouldSetStartupAssembly()
    {
        var builder = Host.CreateApplicationBuilder();
        await builder.ConfigureApplicationAsync(o => o.StartupAssembly = typeof(Assembly2.Assembly2Package).Assembly);
        var host = builder.Build();
        var application = ServiceProviderExtensions.GetRequiredService<AxiomApplication>(host.Services);

        application.StartupAssembly.ShouldBe(typeof(Assembly2.Assembly2Package).Assembly);
        application.Assemblies.ShouldContain(typeof(Assembly2.Assembly2Package).Assembly);
        application.Assemblies.ShouldNotContain(typeof(Assembly3.Assembly3Package).Assembly);
    }

    [Fact]
    public async ValueTask ShouldOverrideDependencyRegistrar()
    {
        var builder = Host.CreateApplicationBuilder();
        await builder.ConfigureApplicationAsync(
            o => o.DependencyRegistrar = new CustomDependencyRegistrar(builder.Services));

        builder.Services.ShouldContain(t => t.ServiceType == typeof(SomeClassRegisterMe));
    }

    [Fact]
    public async ValueTask ShouldOverrideApplicationBuilder()
    {
        var builder = Host.CreateApplicationBuilder();
        await builder.ConfigureApplicationAsync(o => o.ApplicationBuilder = new CustomApplicationBuilder());
        var host = builder.Build();
        var application = ServiceProviderExtensions.GetRequiredService<AxiomApplication>(host.Services);

        application.Id.ShouldBe(Guid.Empty);
        application.Assemblies.Count.ShouldBe(0);
    }

    [Fact]
    public async ValueTask ShouldLoadPlugins()
    {
        var assembly = typeof(Assembly1.Assembly1Package).Assembly;
        var builder = Host.CreateApplicationBuilder();
        await builder.ConfigureApplicationAsync(o =>
        {
            o.StartupAssembly = assembly;
            o.Plugins.Add(new AxiomApplicationAssemblyPlugin(typeof(Assembly2.Assembly2Package).Assembly));
        });
        var host = builder.Build();
        var application = ServiceProviderExtensions.GetRequiredService<AxiomApplication>(host.Services);

        application.StartupAssembly.ShouldBe(assembly);
        application.Assemblies.ShouldContain(assembly);
        application.Assemblies.ShouldContain(typeof(Assembly2.Assembly2Package).Assembly);
        application.Assemblies.ShouldNotContain(typeof(Assembly3.Assembly3Package).Assembly);
    }
}

file class TestService : ITransientService
{
    public int GetNumber() => 1;
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