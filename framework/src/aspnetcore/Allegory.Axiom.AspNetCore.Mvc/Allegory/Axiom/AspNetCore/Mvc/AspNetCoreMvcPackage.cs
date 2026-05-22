using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.AspNetCore.Mvc;

internal sealed class AspNetCoreMvcPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        //builder.Services
        return Task.CompletedTask;
    }
}