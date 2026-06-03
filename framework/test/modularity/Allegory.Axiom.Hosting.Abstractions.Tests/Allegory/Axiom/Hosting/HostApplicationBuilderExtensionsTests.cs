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

public class HostApplicationBuilderExtensionsTests
{
    protected HostApplicationBuilder Builder { get; } = Host.CreateApplicationBuilder();

    [Fact]
    public async Task ShouldConfigureApplication()
    {
        Builder.Services.ShouldNotContain(t => t.ServiceType == typeof(AxiomApplication));

        var application = await Builder.ConfigureApplicationAsync();

        application.Id.ShouldNotBe(Guid.Empty);
        Builder.Services.ShouldContain(t => t.ServiceType == typeof(AxiomApplication));
    }

    [Fact]
    public async Task ShouldSetEntryAssemblyWhenStartupAssemblyIsNull()
    {
        var application = await Builder.ConfigureApplicationAsync();

        application.StartupAssembly.ShouldBe(Assembly.GetEntryAssembly());
    }

    [Fact]
    public async Task ShouldOverrideDependencyRegistrar()
    {
        await Builder.ConfigureApplicationAsync(o => o.DependencyRegistrar = new CustomDependencyRegistrar(Builder.Services));

        Builder.Services.ShouldContain(t => t.ServiceType == typeof(SomeClassRegisterMe));
    }

    [Fact]
    public async Task ShouldOverrideApplicationBuilder()
    {
        var application = await Builder.ConfigureApplicationAsync(o => o.ApplicationBuilder = new CustomApplicationBuilder());

        application.Id.ShouldBe(Guid.Empty);
        application.Assemblies.Count.ShouldBe(0);
    }

    [Fact]
    public async Task ShouldPassPluginsToBuilder()
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

    [Fact]
    public async Task ShouldGetAxiomApplicationWhileConfiguringDependencies()
    {
        AxiomApplication application = null!;

        Builder.AddDeferredAction(builder =>
        {
            application = builder.GetAxiomApplication();
            application.Assemblies.Count.ShouldBeGreaterThan(0);
        });

        await Builder.ConfigureApplicationAsync();
        var host = Builder.Build();
        var serviceApplication = host.Services.GetRequiredService<AxiomApplication>();

        application.ShouldBe(serviceApplication);
    }

    [Fact]
    public async Task ShouldInvokeDeferredAction()
    {
        var wasCalled = false;

        Builder.AddDeferredAction(_ => wasCalled = true);
        await Builder.ConfigureApplicationAsync();

        wasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldInvokeDeferredActionsInOrder()
    {
        var callOrder = new List<int>();

        Builder.AddDeferredAction(_ => callOrder.Add(1));
        Builder.AddDeferredAction(_ => callOrder.Add(2));
        Builder.AddDeferredAction(_ => callOrder.Add(3));
        await Builder.ConfigureApplicationAsync();

        callOrder.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public async Task ShouldPassBuilderInstanceToDeferredAction()
    {
        IHostApplicationBuilder? received = null;

        Builder.AddDeferredAction(b => received = b);
        await Builder.ConfigureApplicationAsync();

        received.ShouldBeSameAs(Builder);
    }

    [Fact]
    public void ShouldThrowWhenDeferredActionIsNull()
    {
        Should.Throw<ArgumentNullException>(() => Builder.AddDeferredAction(null!));
    }

    [Fact]
    public async Task ShouldClearDeferredActionsAfterExecution()
    {
        Builder.AddDeferredAction(_ => {});
        await Builder.ConfigureApplicationAsync();

        var actions = HostApplicationBuilderExtensions.BuilderProperties
            .GetOrCreateValue(Builder).DeferredActions;
        actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldNotShareDeferredActionsBetweenBuilders()
    {
        var builder2 = Host.CreateApplicationBuilder();
        var wasCalled = false;

        Builder.AddDeferredAction(_ => wasCalled = true);
        await builder2.ConfigureApplicationAsync();

        wasCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldInvokeBuilderAction()
    {
        var wasCalled = false;
        var builderInstance = new TestBuilder();

        Builder.AddBuilder(builderInstance);
        Builder.AddBuilderAction<TestBuilder>(_ => wasCalled = true);

        await Builder.ConfigureApplicationAsync();

        wasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldInvokeBuilderActionsInOrder()
    {
        var callOrder = new List<int>();
        var builderInstance = new TestBuilder();

        Builder.AddBuilder(builderInstance);
        Builder.AddBuilderAction<TestBuilder>(_ => callOrder.Add(1));
        Builder.AddBuilderAction<TestBuilder>(_ => callOrder.Add(2));
        Builder.AddBuilderAction<TestBuilder>(_ => callOrder.Add(3));

        await Builder.ConfigureApplicationAsync();

        callOrder.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public async Task ShouldPassBuilderInstanceToAction()
    {
        var builderInstance = new TestBuilder();
        TestBuilder? received = null;

        Builder.AddBuilder(builderInstance);
        Builder.AddBuilderAction<TestBuilder>(b => received = b);

        await Builder.ConfigureApplicationAsync();

        received.ShouldBeSameAs(builderInstance);
    }

    [Fact]
    public async Task ShouldThrowWhenBuilderActionRegisteredWithoutBuilder()
    {
        var wasCalled = false;

        Builder.AddBuilderAction<TestBuilder>(_ => wasCalled = true);

        await Should.ThrowAsync<ArgumentNullException>(Builder.ConfigureApplicationAsync());

        wasCalled.ShouldBeFalse();
    }

    [Fact]
    public void ShouldThrowWhenSameBuilderTypeRegisteredTwice()
    {
        Builder.AddBuilder(new TestBuilder());

        Should.Throw<InvalidOperationException>(() => Builder.AddBuilder(new TestBuilder()));
    }

    [Fact]
    public async Task ShouldClearBuilderContextsAfterExecution()
    {
        Builder.AddBuilder(new TestBuilder());
        Builder.AddBuilderAction<TestBuilder>(_ => {});

        var contexts = HostApplicationBuilderExtensions.BuilderProperties.GetOrCreateValue(Builder).BuilderContexts;
        contexts.ShouldNotBeEmpty();

        await Builder.ConfigureApplicationAsync();

        contexts.ShouldBeEmpty();
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
    protected override Task<AxiomApplication> BuildAsync()
    {
        return Task.FromResult(new AxiomApplication(Guid.Empty, Context.StartupAssembly, []));
    }
}

file class TestBuilder;