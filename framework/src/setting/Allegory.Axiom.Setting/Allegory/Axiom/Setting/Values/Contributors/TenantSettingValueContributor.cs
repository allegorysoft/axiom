using System.Threading.Tasks;
using Allegory.Axiom.MultiTenancy;

namespace Allegory.Axiom.Setting;

public class TenantSettingValueContributor(
    ISettingValueStore store,
    ITenantContextAccessor tenantContextAccessor) :
    ISettingValueContributor
{
    public string Name => ISettingValueContributor.Tenant;
    protected ISettingValueStore Store { get; } = store;
    protected ITenantContextAccessor TenantContextAccessor { get; } = tenantContextAccessor;

    public virtual ValueTask<SettingValue?> FindAsync(SettingDefinition definition)
    {
        return Store.FindAsync(
            definition.Name,
            new SettingValueIdentifier(Name, TenantContextAccessor.Current?.Id.ToString()));
    }
}