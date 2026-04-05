using System.Threading.Tasks;
using Microsoft.Testing.Platform.Services;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkManagerTests : IntegrationTestBase
{
    [Fact]
    public async Task Test()
    {
        var manager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
    }
}