using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Hosting;

public interface IPostConfigureApplication
{
    static abstract Task PostConfigureAsync(IHostApplicationBuilder builder);
}