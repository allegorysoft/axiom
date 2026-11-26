using System.Threading.Tasks;

namespace Allegory.Axiom.Setting;

public interface ISettingValueManager
{
    ValueTask<T?> FindAsync<T>(string name);
    ValueTask<T?> FindAsync<T>(SettingDefinition definition);
    ValueTask<SettingValue?> FindAsync(string name);
    ValueTask<SettingValue?> FindAsync(SettingDefinition definition);

    Task SetAsync<T>(
        string name,
        SettingValueIdentifier identifier,
        T value,
        bool checkFallbackValue = true);

    Task SetAsync<T>(
        SettingDefinition definition,
        SettingValueIdentifier identifier,
        T value,
        bool checkFallbackValue = true);
}