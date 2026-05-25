using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Hosting;

public class HostExtensionsTests : IAsyncLifetime
{
    protected HostApplicationBuilder Builder { get; } = Host.CreateApplicationBuilder();

    public async ValueTask InitializeAsync() => await Builder.ConfigureApplicationAsync();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ShouldInitializeApplication()
    {
        var host = Builder.Build();
        await host.InitializeApplicationAsync();

        HostingAbstractionsTestsPackage.InitializeApplication.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldInvokeBuilderAction()
    {
        var wasCalled = false;
        var builderInstance = new TestBuilder();

        var host = Builder.Build();
        host.AddBuilder(builderInstance);
        host.AddBuilderAction<TestBuilder>(_ => wasCalled = true);

        await host.InitializeApplicationAsync();

        wasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldInvokeBuilderActionsInOrder()
    {
        var callOrder = new List<int>();
        var builderInstance = new TestBuilder();

        var host = Builder.Build();
        host.AddBuilder(builderInstance);
        host.AddBuilderAction<TestBuilder>(_ => callOrder.Add(1));
        host.AddBuilderAction<TestBuilder>(_ => callOrder.Add(2));
        host.AddBuilderAction<TestBuilder>(_ => callOrder.Add(3));

        await host.InitializeApplicationAsync();

        callOrder.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public async Task ShouldPassBuilderInstanceToAction()
    {
        var builderInstance = new TestBuilder();
        TestBuilder? received = null;

        var host = Builder.Build();
        host.AddBuilder(builderInstance);
        host.AddBuilderAction<TestBuilder>(b => received = b);

        await host.InitializeApplicationAsync();

        received.ShouldBeSameAs(builderInstance);
    }

    [Fact]
    public async Task ShouldThrowWhenBuilderActionRegisteredWithoutBuilder()
    {
        var wasCalled = false;

        var host = Builder.Build();
        host.AddBuilderAction<TestBuilder>(_ => wasCalled = true);

        await Should.ThrowAsync<ArgumentNullException>(host.InitializeApplicationAsync());

        wasCalled.ShouldBeFalse();
    }

    [Fact]
    public void ShouldThrowWhenSameBuilderTypeRegisteredTwice()
    {
        var host = Builder.Build();
        host.AddBuilder(new TestBuilder());

        Should.Throw<InvalidOperationException>(() => host.AddBuilder(new TestBuilder()));
    }

    [Fact]
    public async Task ShouldClearBuilderContextsAfterExecution()
    {
        var host = Builder.Build();
        host.AddBuilder(new TestBuilder());
        host.AddBuilderAction<TestBuilder>(_ => {});

        var contexts = HostExtensions.HostProperties.GetOrCreateValue(host).BuilderContexts;
        contexts.ShouldNotBeEmpty();
        
        await host.InitializeApplicationAsync();

        contexts.ShouldBeEmpty();
    }
}

file class TestBuilder;