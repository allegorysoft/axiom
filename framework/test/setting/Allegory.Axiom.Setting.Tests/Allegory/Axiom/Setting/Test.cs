using System.Text.Json;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.Caching.Hybrid;
using Xunit;

namespace Allegory.Axiom.Setting;

public class Test(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected ISettingDefinitionProvider Provider => fixture.Service<ISettingDefinitionProvider>();
    protected HybridCache Cache => fixture.Service<HybridCache>();

    [Fact]
    public async Task Test1()
    {
        var element = JsonElement.Parse("""
                                        {
                                           "Name": "Ahmet",
                                           "Age": 29
                                        }
                                        """);

        var definition = new SettingDefinition("Person", element);

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

public class SmtpSettingDefinitionContributor : ISettingDefinitionContributor, ISingletonService
{
    public void Contribute(SettingDefinitionContext context)
    {
        for (int i = 0; i < 500; i++)
        {
            context.Add(SettingDefinition.Create($"smtp{i}", new SmtpSetting
            {
                Host = "127.0.0.1",
                Port = 25
            }));
        }
    }
}

public class SmtpSetting
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
}