using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Allegory.Axiom.Localization;

public class Test(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task T()
    {
        var x = typeof(string).FullName;

    }
}