using System;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Caching;
using Allegory.Axiom.MultiTenancy;
using Xunit;

namespace Allegory.Axiom.Localization;

public class Test(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task T()
    {
        var x = typeof(string).FullName;
        var y = fixture.Service<ICache>();

        var r = await y.GetOrCreateAsync<string?>(
            "a",
            static (_) => ValueTask.FromResult<string?>(null),
            cancellationToken: TestContext.Current.CancellationToken);
        
    }
}