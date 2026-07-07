using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqConnectionFactoryTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected RabbitMqConnectionFactory Factory => fixture.Service<RabbitMqConnectionFactory>();

    [Fact]
    public async Task ShouldGetConnection()
    {
        var connection = await Factory.GetAsync(RabbitMqOptions.DefaultConnectionName);
        connection.ShouldNotBeNull();
        connection.Connection.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnSameConnectionInstanceForSameName()
    {
        var first = await Factory.GetAsync(RabbitMqOptions.DefaultConnectionName);
        var second = await Factory.GetAsync(RabbitMqOptions.DefaultConnectionName);

        second.ShouldBeSameAs(first);
    }

    [Fact]
    public async Task ShouldThrowForUnknownConnectionName()
    {
        await Should.ThrowAsync<InvalidOperationException>(() =>
            Factory.GetAsync("does-not-exist").AsTask());
    }

    [Fact]
    public async Task ShouldCreateDistinctConnectionsForDistinctNames()
    {
        var connection1 = await Factory.GetAsync(RabbitMqOptions.DefaultConnectionName);
        var connection2 = await Factory.GetAsync(RabbitMqTestsPackage.SecondConnectionName);

        connection1.ShouldNotBeSameAs(connection2);
    }

    [Fact]
    public async Task ShouldHandleConcurrentGetAsyncCallsSafely()
    {
        const int degree = 8;
        var results = new RabbitMqConnection[degree];

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