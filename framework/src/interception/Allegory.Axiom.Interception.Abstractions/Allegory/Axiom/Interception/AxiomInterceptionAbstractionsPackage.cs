using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Interception;

internal class AxiomInterceptionAbstractionsPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddPostConfigureAction(ApplyInterceptors);
        return ValueTask.CompletedTask;
    }

    private static void ApplyInterceptors(IServiceCollection collection)
    {
        ServiceInterceptorBinder.Apply(collection, collection.Interceptors);
    }
}