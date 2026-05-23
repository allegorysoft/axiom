using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ShouldInvokePostConfigureAction()
    {
        var services = new ServiceCollection();
        var wasCalled = false;

        services.AddPostConfigureAction(_ => wasCalled = true);
        services.ExecutePostConfigureActions();

        wasCalled.ShouldBeTrue();
    }

    [Fact]
    public void ShouldInvokePostConfigureActionsInOrder()
    {
        var services = new ServiceCollection();
        var callOrder = new List<int>();

        services.AddPostConfigureAction(_ => callOrder.Add(1));
        services.AddPostConfigureAction(_ => callOrder.Add(2));
        services.AddPostConfigureAction(_ => callOrder.Add(3));
        services.ExecutePostConfigureActions();

        callOrder.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void ShouldInvokeBuilderAction()
    {
        var services = new ServiceCollection();
        var wasCalled = false;

        services.AddBuilderAction<TestBuilder>(_ => wasCalled = true);
        services.ExecuteBuilderActions(new TestBuilder());

        wasCalled.ShouldBeTrue();
    }

    [Fact]
    public void ShouldInvokeBuilderActionsInOrder()
    {
        var services = new ServiceCollection();
        var callOrder = new List<int>();

        services.AddBuilderAction<TestBuilder>(_ => callOrder.Add(1));
        services.AddBuilderAction<TestBuilder>(_ => callOrder.Add(2));
        services.AddBuilderAction<TestBuilder>(_ => callOrder.Add(3));
        services.ExecuteBuilderActions(new TestBuilder());

        callOrder.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void ShouldPassBuilderInstanceToAction()
    {
        var services = new ServiceCollection();

        TestBuilder? received = null;
        services.AddBuilderAction<TestBuilder>(b => received = b);

        var builder = new TestBuilder();
        services.ExecuteBuilderActions(builder);

        received.ShouldBeSameAs(builder);
    }

    [Fact]
    public void ShouldNotInvokeBuilderActionsWhenNoneRegistered()
    {
        var services = new ServiceCollection();
        var wasCalled = false;

        // no AddBuilderAction call
        services.ExecuteBuilderActions(new TestBuilder());

        wasCalled.ShouldBeFalse();
    }

    [Fact]
    public void ShouldScopeBuilderActionsByType()
    {
        var services = new ServiceCollection();

        var testBuilderCalled = false;
        services.AddBuilderAction<TestBuilder>(_ => testBuilderCalled = true);

        var otherBuilderCalled = false;
        services.AddBuilderAction<OtherBuilder>(_ => otherBuilderCalled = true);

        services.ExecuteBuilderActions(new OtherBuilder());

        testBuilderCalled.ShouldBeFalse();
        otherBuilderCalled.ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotShareExtraPropertiesBetweenServiceCollections()
    {
        var services1 = new ServiceCollection();
        var services2 = new ServiceCollection();
        var service1ExtraProperties = ServiceCollectionExtensions.ExtraProperties.GetOrCreateValue(services1);
        var service2ExtraProperties = ServiceCollectionExtensions.ExtraProperties.GetOrCreateValue(services2);

        services1.AddPostConfigureAction(_ => {});
        services1.AddBuilderAction<TestBuilder>(_ => {});

        service1ExtraProperties.PostConfigureActions.Count.ShouldBe(1);
        service1ExtraProperties.BuilderActions.Count.ShouldBe(1);
        service2ExtraProperties.PostConfigureActions.Count.ShouldBe(0);
        service2ExtraProperties.BuilderActions.Count.ShouldBe(0);
    }
}

file class TestBuilder;

file class OtherBuilder;