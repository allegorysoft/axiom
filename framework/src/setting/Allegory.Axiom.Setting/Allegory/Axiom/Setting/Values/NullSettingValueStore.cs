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

    public ValueTask<SettingValue?> FindAsync(string name, SettingValueIdentifier identifier)
    {
        Logger.LogError(
            "Setting value store is not configured. Cannot retrieve setting {SettingName} for {Identifier}",
            name,
            identifier);

        return ValueTask.FromResult<SettingValue?>(null);
    }

    public Task SetAsync(string name, SettingValueIdentifier identifier, SettingValue value)
    {
        Logger.LogError(
            "Setting value store is not configured. Cannot persist setting {SettingName} for {Identifier}",
            name,
            identifier);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string name, SettingValueIdentifier identifier)
    {
        Logger.LogError(
            "Setting value store is not configured. Cannot remove setting {SettingName} for {Identifier}",
            name,
            identifier);

        return Task.CompletedTask;
    }
}