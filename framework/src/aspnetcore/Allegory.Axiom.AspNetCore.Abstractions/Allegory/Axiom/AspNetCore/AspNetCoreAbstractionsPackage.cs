using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.AspNetCore;

internal sealed class AspNetCoreAbstractionsPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        AssemblyDependencyRegistrar.IgnoredServiceTypes.Add(typeof(IEndpointFilter));

        return Task.CompletedTask;
    }
}