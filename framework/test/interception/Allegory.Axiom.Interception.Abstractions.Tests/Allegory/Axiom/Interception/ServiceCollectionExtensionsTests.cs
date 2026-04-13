using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Interception;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ShouldAddInterceptor()
    {
        var services = new ServiceCollection();

        services.AddInterceptor<Interceptor1>(_ => true);
        services.AddInterceptor(typeof(Interceptor1), _ => true);

        ServiceCollectionExtensions.ExtraProperties.GetOrCreateValue(services).Interceptors.Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldThrowExceptionWhenInterceptorTypeIsNotInheritedFromInterface()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentException>(() => services.AddInterceptor(typeof(int), _ => true));
    }
}

file class Interceptor1 : IInterceptor
{
    public Task InterceptAsync(IInterceptorContext context) => context.ProceedAsync();
}