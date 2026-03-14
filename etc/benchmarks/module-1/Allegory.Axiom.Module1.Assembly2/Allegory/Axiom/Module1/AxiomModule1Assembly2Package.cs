using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Module1;

internal sealed class AxiomModule1Assembly2Package : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        return ValueTask.CompletedTask;
    }
}