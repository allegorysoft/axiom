using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using Xunit;

namespace Allegory.Axiom.Settings;

public class Test(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task Test1()
    {
        using var document = JsonDocument.Parse("""
                                                {
                                                   "Name": "Ahmet",
                                                   "Age": 29
                                                }
                                                """);
        document.RootElement.GetRawText();


        var element = JsonElement.Parse("""
                                        {
                                           "Name": "Ahmet",
                                           "Age": 29
                                        }
                                        """);


        var definition = new SettingDefinition
        {
            Name = "Person",
            DefaultValue = element
        };

        var cache = fixture.Service<HybridCache>();

        var x = await cache.GetOrCreateAsync(
            "p:1",
            _ => ValueTask.FromResult(definition),
            cancellationToken: TestContext.Current.CancellationToken);

        var y = await cache.GetOrCreateAsync(
            "p:1",
            _ => ValueTask.FromResult(definition),
            cancellationToken: TestContext.Current.CancellationToken);
    }
}

public struct a
{
    
}