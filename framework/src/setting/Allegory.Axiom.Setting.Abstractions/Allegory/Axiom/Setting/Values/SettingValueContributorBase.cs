using System.Threading.Tasks;

namespace Allegory.Axiom.Setting;

public abstract class SettingValueContributorBase(ISettingValueStore store) : ISettingValueContributor
{
    public abstract string Name { get; }
    protected ISettingValueStore Store { get; } = store;

    public virtual async ValueTask<SettingValue?> FindAsync(SettingDefinition definition)
    {
        var key = await GetKeyAsync(definition);

        if (key == null)
        {
            return null;
        }

        return await Store.FindAsync(definition.Name, new SettingValueIdentifier(Name, key));
    }

    protected abstract ValueTask<string?> GetKeyAsync(SettingDefinition definition);
}