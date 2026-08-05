using System.Threading.Tasks;
using Allegory.Axiom.MultiTenancy;
using Xunit;

namespace Allegory.Axiom.Data;

public class ConnectionStringProviderTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string Tenant1 = "T-1", Tenant2 = "T-2";

    protected IConnectionStringProvider ConnectionStringProvider => fixture.Service<IConnectionStringProvider>();
    protected ITenantStore TenantStore => fixture.Service<ITenantStore>();
    protected ITenantContextAccessor TenantContextAccessor => fixture.Service<ITenantContextAccessor>();

    [Fact]
    public async Task Test()
    {
        TenantContextAccessor.Set(await TenantStore.GetAsync(Tenant1));

        var connection = await ConnectionStringProvider.GetAsync("");
    }
}