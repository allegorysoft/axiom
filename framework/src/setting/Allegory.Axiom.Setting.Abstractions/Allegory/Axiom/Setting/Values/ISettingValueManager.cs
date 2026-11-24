using System.Threading.Tasks;

namespace Allegory.Axiom.Setting;

public interface ISettingValueManager
{
    ValueTask<T?> FindAsync<T>(string name);

    ValueTask<T?> FindAsync<T>(SettingDefinition definition);

    ValueTask<SettingValue?> FindAsync(string name);

    ValueTask<SettingValue?> FindAsync(SettingDefinition definition);
}