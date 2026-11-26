using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Allegory.Axiom.Setting;

internal sealed class DefaultSettingValueContributor : ISettingValueContributor
{
    public const string ContributorName = "D";

    public string Name => ContributorName;
    public bool IsEncrypted => false;
    public ConcurrentDictionary<SettingDefinition, SettingValue?> Values { get; } = new();

    public ValueTask<SettingValue?> FindAsync(SettingDefinition definition)
    {
        var value = Values.GetOrAdd(
            definition,
            static d => d.DefaultValue.HasValue ? new SettingValue(d.DefaultValue.Value) : null);

        return new ValueTask<SettingValue?>(value);
    }
}