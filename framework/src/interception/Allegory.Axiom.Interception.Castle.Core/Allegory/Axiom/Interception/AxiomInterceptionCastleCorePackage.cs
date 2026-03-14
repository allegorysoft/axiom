using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Interception;

internal class AxiomInterceptionCastleCorePackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddPostConfigureAction(RegisterCastleAdaptersByInterceptorLifetime);
        return ValueTask.CompletedTask;
    }

    private static void RegisterCastleAdaptersByInterceptorLifetime(IServiceCollection serviceCollection)
    {
        var interceptors = serviceCollection.Where(
            x => typeof(IAxiomInterceptor).IsAssignableFrom(x.ServiceType) &&
                 x.Lifetime > ServiceLifetime.Singleton)
            .ToList();

        foreach (var interceptor in interceptors)
        {
            var castleDeterminationType = typeof(AxiomInterceptorDeterminationCastleAdapter<>)
                .MakeGenericType(interceptor.ServiceType);
            serviceCollection.Add(
                ServiceDescriptor.Describe(castleDeterminationType, castleDeterminationType, interceptor.Lifetime));

            var castleAsyncType = typeof(AxiomInterceptorCastleAdapter<>).MakeGenericType(interceptor.ServiceType);
            serviceCollection.Add(
                ServiceDescriptor.Describe(castleAsyncType, castleAsyncType, interceptor.Lifetime));
        }
    }
}