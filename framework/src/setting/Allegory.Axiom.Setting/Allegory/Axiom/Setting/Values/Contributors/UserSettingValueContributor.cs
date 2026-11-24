using System.Security.Principal;
using System.Threading.Tasks;
using Allegory.Axiom.Security.Principal;

namespace Allegory.Axiom.Setting;

public class UserSettingValueContributor(
    ISettingValueStore store,
    IPrincipalAccessor principalAccessor) :
    SettingValueContributorBase(store)
{
    public override string Name => ISettingValueContributor.User;
    protected IPrincipalAccessor PrincipalAccessor { get; } = principalAccessor;

    protected override ValueTask<string?> GetKeyAsync(SettingDefinition definition)
    {
        return ValueTask.FromResult(PrincipalAccessor.Current?.Identity?.FindNameIdentifier());
    }
}