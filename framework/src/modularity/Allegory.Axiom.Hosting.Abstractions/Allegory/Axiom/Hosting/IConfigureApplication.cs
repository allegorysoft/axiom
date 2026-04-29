using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Hosting;

public interface IConfigureApplication
{
    static abstract Task ConfigureAsync(IHostApplicationBuilder builder);
}