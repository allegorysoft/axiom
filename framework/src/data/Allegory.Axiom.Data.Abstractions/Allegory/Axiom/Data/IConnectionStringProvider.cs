using System.Threading.Tasks;

namespace Allegory.Axiom.Data;

public interface IConnectionStringProvider
{
    ValueTask<string> GetAsync(string name);
    ValueTask<string?> FindAsync(string name);
}