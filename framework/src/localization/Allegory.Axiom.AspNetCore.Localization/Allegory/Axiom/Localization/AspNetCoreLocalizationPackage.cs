using System.Threading.Tasks;
using Allegory.Axiom.AspNetCore;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Localization;

internal sealed class AspNetCoreLocalizationPackage : IInitializeApplication
{
    public static Task InitializeAsync(IHost host)
    {
        host.GetDefaultRouteGroupBuilder().MapLocalizationEndpoints();

        return Task.CompletedTask;
    }
}