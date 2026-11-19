using System.Collections.Immutable;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.Settings;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class InProcessSettingDefinitionStore(
    ISettingDefinitionProvider provider) :
    ISettingDefinitionStore, ISingletonService
{
    protected ISettingDefinitionProvider Provider { get; } = provider;

    public virtual ValueTask<ImmutableArray<SettingDefinition>> GetListAsync()
    {
        return ValueTask.FromResult(Provider.Settings.Values);
    }

    public virtual ValueTask<SettingDefinition?> FindAsync(string name)
    {
        Provider.Settings.TryGetValue(name, out var value);
        return ValueTask.FromResult(value);
    }
}