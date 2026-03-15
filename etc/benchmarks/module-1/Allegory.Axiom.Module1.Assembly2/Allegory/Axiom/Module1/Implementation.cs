using System.Threading.Tasks;

namespace Allegory.Axiom.Module1;

public class Implementation : IImplementation
{
    public void Do() {}
    public Task DoAsync() => Task.CompletedTask;
}