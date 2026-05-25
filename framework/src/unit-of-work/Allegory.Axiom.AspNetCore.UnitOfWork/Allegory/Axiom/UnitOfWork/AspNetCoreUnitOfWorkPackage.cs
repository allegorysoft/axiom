using System.Threading.Tasks;
using Allegory.Axiom.AspNetCore;
using Allegory.Axiom.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.UnitOfWork;

internal sealed class AspNetCoreUnitOfWorkPackage : IInitializeApplication
{
    public static Task InitializeAsync(IHost host)
    {
        var builder = host.GetDefaultRouteGroupBuilder();
        builder.AddEndpointFilter<UnitOfWorkEndpointFilter>();

        return Task.CompletedTask;
    }
}