using System.Threading.Tasks;
using Allegory.Axiom.AspNetCore;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Settings;

internal sealed class AspNetCoreSettingsPackage : IInitializeApplication
{
    public static Task InitializeAsync(IHost host)
    {
        host.GetDefaultRouteGroupBuilder().MapSettingEndpoints();

        return Task.CompletedTask;
    }
}