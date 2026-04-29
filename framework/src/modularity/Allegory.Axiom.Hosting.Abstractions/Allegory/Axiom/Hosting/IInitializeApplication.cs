using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Hosting;

public interface IInitializeApplication
{
    static abstract Task InitializeAsync(IHost host);
}