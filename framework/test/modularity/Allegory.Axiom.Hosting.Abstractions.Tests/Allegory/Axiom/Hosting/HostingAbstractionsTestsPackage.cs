using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Hosting;

internal sealed class HostingAbstractionsTestsPackage :
    IConfigureApplication,
    IPostConfigureApplication,
    IInitializeApplication
{
    public static bool ConfigureApplication, PostConfigureApplication, InitializeApplication;

    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        ConfigureApplication = true;
        return Task.CompletedTask;
    }

    public static Task PostConfigureAsync(IHostApplicationBuilder builder)
    {
        PostConfigureApplication = true;
        return Task.CompletedTask;
    }

    public static Task InitializeAsync(IHost host)
    {
        InitializeApplication = true;
        return Task.CompletedTask;
    }
}