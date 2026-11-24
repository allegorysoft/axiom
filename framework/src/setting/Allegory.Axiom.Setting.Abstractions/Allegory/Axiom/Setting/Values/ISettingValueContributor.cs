using System.Threading.Tasks;

namespace Allegory.Axiom.Setting;

public interface ISettingValueContributor
{
    const string Configuration = "C";
    const string Global = "G";
    const string Tenant = "T";
    const string User = "U";

    string Name { get; }

    ValueTask<SettingValue?> FindAsync(SettingDefinition definition);
}