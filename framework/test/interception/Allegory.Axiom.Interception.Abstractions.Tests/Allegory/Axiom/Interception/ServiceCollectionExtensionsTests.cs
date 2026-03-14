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
}

file class Interceptor1 : IAxiomInterceptor
{
    public Task InterceptAsync(IAxiomInterceptorContext context) => context.ProceedAsync();
}