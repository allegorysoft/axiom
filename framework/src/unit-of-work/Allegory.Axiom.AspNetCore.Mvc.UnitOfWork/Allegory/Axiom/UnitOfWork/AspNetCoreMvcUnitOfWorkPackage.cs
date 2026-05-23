using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.UnitOfWork;

internal sealed class AspNetCoreMvcUnitOfWorkPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.AddBuilderAction<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddMvcOptions(mvcOptions =>
            {
                mvcOptions.Filters.Add<UnitOfWorkActionFilter>();
            });
        });

        return Task.CompletedTask;
    }
}