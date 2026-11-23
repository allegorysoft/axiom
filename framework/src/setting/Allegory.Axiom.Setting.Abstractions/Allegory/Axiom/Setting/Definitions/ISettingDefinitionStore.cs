using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Allegory.Axiom.Setting;

public interface ISettingDefinitionStore
{
    ValueTask<ImmutableArray<SettingDefinition>> GetListAsync();
    ValueTask<SettingDefinition?> FindAsync(string name);
}