using System.Threading.Tasks;
using Allegory.Axiom.MultiTenancy;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Data;

public class ConnectionStringProviderTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    private const string Tenant1 = "T-1", Tenant2 = "T-2";

    protected IConnectionStringProvider Provider => fixture.Service<IConnectionStringProvider>();
    protected ITenantStore TenantStore => fixture.Service<ITenantStore>();
    protected ITenantContextAccessor TenantContextAccessor => fixture.Service<ITenantContextAccessor>();

    // Host

    [Fact]
    public async Task ShouldResolveConnectionString()
    {
        var connection1 = await Provider.GetAsync("AppDb1");
        var connection2 = await Provider.GetAsync("AppDb2");

        connection1.ShouldBe("host-app-db-1");
        connection2.ShouldBe("host-app-db-2");
    }

    [Fact]
    public async Task ShouldResolveContextSpecifiedConnectionString()
    {
        var connection1 = await Provider.GetAsync("TenantAgnosticGroupedDb1");
        var connection2 = await Provider.GetAsync("TenantAgnosticGroupedDb2");
        var connection3 = await Provider.GetAsync("GroupedDb1");
        var connection4 = await Provider.GetAsync("GroupedDb2");

        connection1.ShouldBe("host-tenant-agnostic-app-group");
        connection2.ShouldBe("host-tenant-agnostic-app-group");
        connection3.ShouldBe("host-app-group");
        connection4.ShouldBe("host-app-group");
    }

    // Tenant

    [Fact]
    public async Task ShouldResolveDefaultConnectionStringForTenant()
    {
        TenantContextAccessor.Set(await TenantStore.GetAsync(Tenant1));

        var connection = await Provider.GetAsync(IConnectionStringProvider.DefaultName);

        connection.ShouldBe("tenant1-app-default");
    }

    [Fact]
    public async Task ShouldResolveSpecifiedConnectionStringForTenant()
    {
        TenantContextAccessor.Set(await TenantStore.GetAsync(Tenant1));

        var connection = await Provider.GetAsync("AppDb1");

        connection.ShouldBe("tenant1-app-db-1");
    }

    [Fact]
    public async Task ShouldResolveContextSpecifiedConnectionStringForTenant()
    {
        TenantContextAccessor.Set(await TenantStore.GetAsync(Tenant1));

        // Tenant-agnostic contexts always use the host-side connection strings from IConfiguration.
        var connection1 = await Provider.GetAsync("TenantAgnosticGroupedDb1");
        var connection2 = await Provider.GetAsync("TenantAgnosticGroupedDb2");
        connection1.ShouldBe("host-tenant-agnostic-app-group");
        connection2.ShouldBe("host-tenant-agnostic-app-group");

        var connection3 = await Provider.GetAsync("GroupedDb1");
        var connection4 = await Provider.GetAsync("GroupedDb2");
        connection3.ShouldBe("tenant1-app-group");
        connection4.ShouldBe("tenant1-app-group");
    }

    [Fact]
    public async Task ShouldFallbackToTenantDefaultConnectionStringWhenNamedConnectionMissing()
    {
        TenantContextAccessor.Set(await TenantStore.GetAsync(Tenant1));

        var connection = await Provider.GetAsync("NotFoundDb");

        connection.ShouldBe("tenant1-app-default");
    }

    // Tenant with no connection strings

    [Fact]
    public async Task ShouldUseHostConnectionStringsWhenTenantHasNoConfiguredConnections()
    {
        TenantContextAccessor.Set(await TenantStore.GetAsync(Tenant2));

        var connection1 = await Provider.GetAsync("AppDb1");
        var connection2 = await Provider.GetAsync("TenantAgnosticGroupedDb1");
        var connection3 = await Provider.GetAsync("GroupedDb1");

        connection1.ShouldBe("host-app-db-1");
        connection2.ShouldBe("host-tenant-agnostic-app-group");
        connection3.ShouldBe("host-app-group");
    }
}