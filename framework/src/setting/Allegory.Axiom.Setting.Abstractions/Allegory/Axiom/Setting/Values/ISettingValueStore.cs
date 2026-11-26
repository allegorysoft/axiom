using System.Threading.Tasks;

namespace Allegory.Axiom.Setting;

public interface ISettingValueStore
{
    ValueTask<SettingValue?> FindAsync(string name, SettingValueIdentifier identifier);

    Task SetAsync(string name, SettingValueIdentifier identifier, SettingValue value);

    Task RemoveAsync(string name, SettingValueIdentifier identifier);
}