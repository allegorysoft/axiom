using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Assembly3;

public class Assembly3Package : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder) => Task.CompletedTask;
}