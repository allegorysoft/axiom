using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqClientFactoryTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected RabbitMqClientFactory Factory => fixture.Service<RabbitMqClientFactory>();

    [Fact]
    public async Task ShouldGetClient()
    {
        var client = await Factory.GetAsync(RabbitMqOptions.DefaultConnectionName);
        client.ShouldNotBeNull();
        client.Connection.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnSameClientInstanceForSameName()
    {
        var first = await Factory.GetAsync(RabbitMqOptions.DefaultConnectionName);
        var second = await Factory.GetAsync(RabbitMqOptions.DefaultConnectionName);

        second.ShouldBe(first);
    }

    [Fact]
    public async Task ShouldThrowForUnknownConnectionName()
    {
        await Should.ThrowAsync<InvalidOperationException>(() =>
            Factory.GetAsync("does-not-exist").AsTask());
    }

    [Fact]
    public async Task ShouldCreateDistinctClientsForDistinctNames()
    {
        var client1 = await Factory.GetAsync(RabbitMqOptions.DefaultConnectionName);
        var client2 = await Factory.GetAsync(RabbitMqTestsPackage.SecondConnectionName);

        client1.ShouldNotBe(client2);
    }

    [Fact]
    public async Task ShouldHandleConcurrentGetAsyncCallsSafely()
    {
        const int degree = 8;
        var results = new RabbitMqClient[degree];

        await Parallel.ForAsync(0, degree, async (i, _) =>
        {
            results[i] = await Factory.GetAsync(RabbitMqOptions.DefaultConnectionName);
        });

        for (var i = 1; i < degree; i++)
        {
            results[i].ShouldBeSameAs(results[0]);
        }
    }
}