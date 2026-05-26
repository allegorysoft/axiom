using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Interception;

internal sealed class InterceptionPackage : IConfigureApplication
{
    static InterceptionPackage()
    {
        AssemblyDependencyRegistrar.IgnoredServiceTypes.Add(typeof(IInterceptor));
    }

    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.AddDeferredAction(static b => ServiceInterceptorBinder.Apply(b.Services));

        return Task.CompletedTask;
    }
}