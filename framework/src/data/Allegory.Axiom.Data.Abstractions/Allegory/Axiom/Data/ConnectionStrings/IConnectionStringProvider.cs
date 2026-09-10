using System.Collections.Frozen;
using System.Threading.Tasks;

namespace Allegory.Axiom.Data.ConnectionStrings;

public interface IConnectionStringProvider
{
    const string DefaultName = "Default";

    FrozenDictionary<string, ConnectionStringContextOptions> Contexts { get; }

    ValueTask<string> GetAsync(string name);
    ValueTask<string?> FindAsync(string name);
}