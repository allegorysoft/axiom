using System.Threading.Tasks;
using Xunit;

namespace Allegory.Axiom.FileProviders;

public class FileProviderManagerTests : IntegrationTestBase
{
    [Fact]
    public async Task Test()
    {
        var manager = Service<FileProviderManager>();
    }
}