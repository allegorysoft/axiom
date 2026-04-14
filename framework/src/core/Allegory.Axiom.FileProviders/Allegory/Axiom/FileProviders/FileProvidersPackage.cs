using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.FileProviders;

internal sealed class FileProvidersPackage : IConfigureApplication
{
    public const string ConfigurationSectionName = "FileProvider";

    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<FileProviderOptions>(o =>
        {
            o.Providers.Add(builder.Environment.ContentRootFileProvider);
        });

        builder.Services.Configure<FileProviderOptions>(
            builder.Configuration.GetSection(ConfigurationSectionName));

        return ValueTask.CompletedTask;
    }
}