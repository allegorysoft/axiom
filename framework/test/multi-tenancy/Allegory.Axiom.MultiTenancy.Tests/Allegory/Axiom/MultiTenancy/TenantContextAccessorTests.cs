using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Extensibility;
using Allegory.Axiom.Security.Principal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Allegory.Axiom.MultiTenancy;

public class TenantContextAccessorTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task Test()
    {
        var x = fixture.Host.Services.GetService<IPrincipalAccessor>();
        var provider = fixture.Service<ICurrentTenantProvider>();
        var result = (await provider.TryGetAsync())!;
        var r = result.GetProperty<int>("key-1");
        var r2 = result.GetProperty<int>("key-1");
    }
}

public class TestCurrentTenantIdentifierProvider : ICurrentTenantIdentifierProvider
{
    public ValueTask<string?> TryGetAsync() => ValueTask.FromResult<string?>("t-1");
}
