using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection.Proxy;
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
    public void ShouldNotShareStateBetweenServiceCollections()
    {
        var services1 = new ServiceCollection();
        var services2 = new ServiceCollection();

        services1.AddPostConfigureAction(_ => {});
        services2.ExecutePostConfigureActions();

        ServiceCollectionExtensions.ExtraProperties.GetOrCreateValue(services1).PostConfigureActions.Count.ShouldBe(1);
        ServiceCollectionExtensions.ExtraProperties.GetOrCreateValue(services2).PostConfigureActions.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldAddInterceptor()
    {
        var services = new ServiceCollection();

        services.AddInterceptor<Interceptor1>(_ => true);
        services.AddInterceptor(typeof(Interceptor1), _ => true);

        ServiceCollectionExtensions.ExtraProperties.GetOrCreateValue(services).Interceptors.Count.ShouldBe(2);
    }
}

file class Interceptor1 : IAxiomInterceptor
{
    public Task InterceptAsync(IAxiomInterceptorContext context) => context.ProceedAsync();
}