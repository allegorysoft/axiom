using System.Threading.Tasks;
using Allegory.Axiom.AspNetCore;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Setting;

internal sealed class AspNetCoreSettingPackage : IInitializeApplication
{
    public static Task InitializeAsync(IHost host)
    {
        host.GetDefaultRouteGroupBuilder().MapSettingEndpoints();

        return Task.CompletedTask;
    }
}