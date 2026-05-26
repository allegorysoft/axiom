using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.AspNetCore;

internal sealed class AspNetCorePackage : IConfigureApplication
{
    static AspNetCorePackage()
    {
        AssemblyDependencyRegistrar.IgnoredServiceTypes.Add(typeof(IEndpointFilter));
    }

    public static Task ConfigureAsync(IHostApplicationBuilder builder) => Task.CompletedTask;
}