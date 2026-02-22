using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Assembly3;

public class Assembly3Package : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder) => ValueTask.CompletedTask;
}