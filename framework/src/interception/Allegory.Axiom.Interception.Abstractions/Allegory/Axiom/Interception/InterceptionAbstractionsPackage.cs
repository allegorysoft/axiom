using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Interception;

internal sealed class InterceptionAbstractionsPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        AssemblyDependencyRegistrar.IgnoredServiceTypes.Add(typeof(IInterceptor));
        builder.AddDeferredAction(static b => ServiceInterceptorBinder.Apply(b.Services));

        return Task.CompletedTask;
    }
}