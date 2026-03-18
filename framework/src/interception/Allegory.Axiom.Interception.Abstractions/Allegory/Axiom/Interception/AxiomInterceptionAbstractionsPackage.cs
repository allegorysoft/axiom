using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Interception;

internal class AxiomInterceptionAbstractionsPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddPostConfigureAction(ServiceInterceptorBinder.Apply);
        return ValueTask.CompletedTask;
    }
}