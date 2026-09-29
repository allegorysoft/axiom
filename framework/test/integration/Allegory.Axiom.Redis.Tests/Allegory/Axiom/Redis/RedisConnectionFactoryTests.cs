using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using StackExchange.Redis;
using Xunit;

namespace Allegory.Axiom.Redis;

public class RedisConnectionFactoryTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    public RedisConnectionFactory Factory => fixture.Service<RedisConnectionFactory>();

    [Fact]
    public async Task ShouldGetConnection()
    {
        var connection = await Factory.GetAsync(RedisOptions.DefaultConnectionName);
        connection.ShouldNotBeNull();
        connection.IsConnected.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnSameConnectionInstanceForSameName()
    {
        var first = await Factory.GetAsync(RedisOptions.DefaultConnectionName);
        var second = Factory.Get(RedisOptions.DefaultConnectionName);

        second.ShouldBeSameAs(first);
    }

    [Fact]
    public async Task ShouldThrowForUnknownConnectionName()
    {
        await Should.ThrowAsync<KeyNotFoundException>(() =>
            Factory.GetAsync("does-not-exist").AsTask());
    }

    [Fact]
    public async Task ShouldCreateDistinctConnectionsForDistinctNames()
    {
        var connection1 = await Factory.GetAsync(RedisOptions.DefaultConnectionName);
        var connection2 = await Factory.GetAsync(RedisTestsPackage.SecondConnectionName);

        connection1.ShouldNotBeSameAs(connection2);
    }

    [Fact]
    public async Task ShouldHandleConcurrentGetAsyncCallsSafely()
    {
        const int degree = 8;
        var results = new IConnectionMultiplexer[degree];

        await Parallel.ForAsync(0, degree,
            async (i, _) => { results[i] = await Factory.GetAsync(RedisOptions.DefaultConnectionName); });

        for (var i = 1; i < degree; i++)
        {
            results[i].ShouldBeSameAs(results[0]);
        }
    }
}