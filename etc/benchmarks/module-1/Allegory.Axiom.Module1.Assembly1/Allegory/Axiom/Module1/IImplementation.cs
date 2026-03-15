using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.Module1;

public interface IImplementation : ITransientService
{
    void Do();
    Task DoAsync();
}