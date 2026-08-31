using System.Threading.Tasks;

namespace Allegory.Axiom.Data;

public interface IConnectionStringProvider
{
    const string DefaultName = "Default";

    ValueTask<string> GetAsync(string name);
    ValueTask<string?> FindAsync(string name);
}