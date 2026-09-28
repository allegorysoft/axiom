using Xunit;

namespace Allegory.Axiom.DistributedLocking;

public class DistributedLockTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public void Test()
    {
        var distributedLock = fixture.Service<IDistributedLock>();
    }
}