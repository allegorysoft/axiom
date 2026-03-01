using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ShouldReturnEmptyListWhenNoActionsAdded()
    {
        var services = new ServiceCollection();

        services.PostConfigureActions.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldAddPostConfigureAction()
    {
        var services = new ServiceCollection();
        Action<IServiceCollection> action = s => s.AddSingleton<IDisposable>();

        services.AddPostConfigureAction(action);

        services.PostConfigureActions.ShouldContain(action);
    }

    [Fact]
    public void ShouldAddMultiplePostConfigureActions()
    {
        var services = new ServiceCollection();
        Action<IServiceCollection> action1 = s => s.AddSingleton<IDisposable>();
        Action<IServiceCollection> action2 = s => s.AddScoped<IDisposable>();

        services.AddPostConfigureAction(action1);
        services.AddPostConfigureAction(action2);

        services.PostConfigureActions.Count.ShouldBe(2);
        services.PostConfigureActions.ShouldBe([action1, action2]);
    }

    [Fact]
    public void ShouldIsolateActionsBetweenDifferentCollections()
    {
        var services1 = new ServiceCollection();
        var services2 = new ServiceCollection();

        services1.AddPostConfigureAction(s => s.AddSingleton<IDisposable>());

        services1.PostConfigureActions.Count.ShouldBe(1);
        services2.PostConfigureActions.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldReturnReadOnlyList()
    {
        var services = new ServiceCollection();

        services.PostConfigureActions.ShouldBeAssignableTo<IReadOnlyList<Action<IServiceCollection>>>();
    }

    [Fact]
    public void ShouldExecuteAddedActionsWhenInvoked()
    {
        var services = new ServiceCollection();
        var executed = false;

        services.AddPostConfigureAction(_ => executed = true);
        services.PostConfigureActions[0](services);

        executed.ShouldBeTrue();
    }
}