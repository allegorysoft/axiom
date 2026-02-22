using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Hosting;

public class AxiomHostingAbstractionsTestsPackage :
    IConfigureApplication,
    IPostConfigureApplication,
    IInitializeApplication
{
    public static bool ConfigureApplication, PostConfigureApplication, InitializeApplication;

    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        ConfigureApplication = true;
        return ValueTask.CompletedTask;
    }

    public static ValueTask PostConfigureAsync(IHostApplicationBuilder builder)
    {
        PostConfigureApplication = true;
        return ValueTask.CompletedTask;
    }

    public static ValueTask InitializeAsync(IHost host)
    {
        InitializeApplication = true;
        return ValueTask.CompletedTask;
    }
}