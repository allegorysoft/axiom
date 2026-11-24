using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.Setting;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class NullSettingValueStore(
    ILogger<NullSettingValueStore> logger) :
    ISettingValueStore, ISingletonService
{
    public ILogger<NullSettingValueStore> Logger { get; } = logger;

    public ValueTask<SettingValue?> FindAsync(
        SettingDefinition definition,
        string contributorName,
        string? contributorKey)
    {
        Logger.LogError(
            "Setting value store is not configured. Cannot retrieve setting {SettingName} for contributor {ContributorName} with key {ContributorKey}",
            definition.Name,
            contributorName,
            contributorKey);

        return ValueTask.FromResult<SettingValue?>(null);
    }

    public Task SetAsync<T>(
        SettingDefinition definition,
        string contributorName,
        string? contributorKey,
        T value,
        bool checkFallbackValue = true)
    {
        Logger.LogError(
            "Setting value store is not configured. Cannot persist setting {SettingName} for contributor {ContributorName} with key {ContributorKey}",
            definition.Name,
            contributorName,
            contributorKey);

        return Task.CompletedTask;
    }
}