using Xunit;

namespace Allegory.Axiom.Data.Filtering;

public class FilterSwitchTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected IFilterSwitch Switch => fixture.Service<IFilterSwitch>();

    [Fact]
    public void Test()
    {
    }
}