using System.Threading.Tasks;

namespace Allegory.Axiom.Setting;

public interface ISettingValueStore
{
    ValueTask<SettingValue?> FindAsync(
        SettingDefinition definition,
        string contributorName,
        string? contributorKey);

    Task SetAsync<T>(
        SettingDefinition definition,
        string contributorName,
        string? contributorKey,
        T value,
        bool checkFallbackValue = true);
}