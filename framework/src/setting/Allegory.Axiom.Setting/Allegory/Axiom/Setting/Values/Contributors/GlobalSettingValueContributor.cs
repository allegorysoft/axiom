using System.Threading.Tasks;

namespace Allegory.Axiom.Setting;

public class GlobalSettingValueContributor(ISettingValueStore store) : ISettingValueContributor
{
    public string Name => ISettingValueContributor.Global;
    protected ISettingValueStore Store { get; } = store;

    public virtual ValueTask<SettingValue?> FindAsync(SettingDefinition definition)
    {
        return Store.FindAsync(definition, Name, null);
    }
}