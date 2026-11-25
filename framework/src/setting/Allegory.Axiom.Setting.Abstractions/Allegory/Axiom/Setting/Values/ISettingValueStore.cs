using System.Threading.Tasks;

namespace Allegory.Axiom.Setting;

public interface ISettingValueStore
{
    ValueTask<SettingValue?> FindAsync(
        SettingDefinition definition,
        SettingValueIdentifier identifier);

    Task SetAsync<T>(
        SettingDefinition definition,
        SettingValueIdentifier identifier,
        T value,
        bool checkFallbackValue = true);
}