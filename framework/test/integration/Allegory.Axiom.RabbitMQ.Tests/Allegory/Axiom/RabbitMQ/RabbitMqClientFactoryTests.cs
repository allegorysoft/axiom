using System.Threading.Tasks;
using Xunit;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqClientFactoryTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task Test()
    {
        var factory = fixture.Service<RabbitMqClientFactory>();

        for (var i = 0; i < 5; i++)
        {
            var client = await factory.GetAsync(RabbitMqOptions.DefaultConnectionName);
            await client.GetChannelAsync("ch-1");
            await client.GetChannelAsync("ch-1");
            await client.GetChannelAsync("ch-2");
            await client.GetChannelAsync("ch-3");
            await Task.Delay(1000, TestContext.Current.CancellationToken);
        }
    }
}