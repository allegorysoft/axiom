using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Interception;

internal sealed class InterceptionCastleCorePackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddPostConfigureAction(RegisterCastleAdaptersByInterceptorLifetime);
        return ValueTask.CompletedTask;
    }

    private static void RegisterCastleAdaptersByInterceptorLifetime(IServiceCollection serviceCollection)
    {
        var interceptors = serviceCollection
            .Where(x => typeof(IInterceptor).IsAssignableFrom(x.ServiceType)
                        && x.Lifetime > ServiceLifetime.Singleton)
            .ToList();

        foreach (var interceptor in interceptors)
        {
            var interceptorType = typeof(InterceptorCastleAdapter<>).MakeGenericType(interceptor.ServiceType);
            serviceCollection.Add(
                ServiceDescriptor.Describe(interceptorType, interceptorType, interceptor.Lifetime));

            var determinationInterceptorType = typeof(DeterminationInterceptorCastleAdapter<>)
                .MakeGenericType(interceptor.ServiceType);
            serviceCollection.Add(
                ServiceDescriptor.Describe(determinationInterceptorType, determinationInterceptorType, interceptor.Lifetime));
        }
    }
}