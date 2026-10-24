using System.Threading.Tasks;
using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Localization;

internal sealed class LocalizationEntityFrameworkCorePackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddAxiomDbContext<LocalizationDbContext>();

        return Task.CompletedTask;
    }
}