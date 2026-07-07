using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqConnectionTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected async Task<RabbitMqConnection> GetConnectionAsync()
        => await fixture.Service<RabbitMqConnectionFactory>()
            .GetAsync(RabbitMqOptions.DefaultConnectionName);

    [Fact]
    public async Task ShouldGetChannel()
    {
        var connection = await GetConnectionAsync();
        var channel = await connection.GetChannelAsync("ch-open");

        channel.ShouldNotBeNull();
        channel.Channel.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnSameChannelInstanceForSameName()
    {
        var connection = await GetConnectionAsync();

        var first = await connection.GetChannelAsync("ch-cache");
        var second = await connection.GetChannelAsync("ch-cache");

        second.ShouldBeSameAs(first);
    }

    [Fact]
    public async Task ShouldReturnDistinctChannelsForDistinctNames()
    {
        var connection = await GetConnectionAsync();

        var ch1 = await connection.GetChannelAsync("ch-distinct-1");
        var ch2 = await connection.GetChannelAsync("ch-distinct-2");

        ch2.ShouldNotBeSameAs(ch1);
    }

    [Fact]
    public async Task ShouldHandleConcurrentGetChannelAsyncCallsSafely()
    {
        var connection = await GetConnectionAsync();
        var results = new RabbitMqChannel[8];

        await Parallel.ForAsync(0, 8, async (i, _) =>
        {
            results[i] = await connection.GetChannelAsync("ch-concurrent");
        });

        for (var i = 1; i < 8; i++)
        {
            results[i].ShouldBeSameAs(results[0]);
        }
    }

    [Fact]
    public async Task ShouldRentChannel()
    {
        var connection = await GetConnectionAsync();

        using var lease = await connection.RentChannelAsync("ch-rent");

        lease.Channel.ShouldNotBeNull();
        lease.Channel.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldAllowOnlyOneActiveLeasePerChannelName()
    {
        const int degree = 8;

        var connection = await GetConnectionAsync();
        var enter = new List<int>();
        var outer = new List<int>();

        await Parallel.ForAsync(0, degree, async (_, _) =>
        {
            var number = Random.Shared.Next();
            using var lease = await connection.RentChannelAsync("ch-exclusive");
            enter.Add(number);
            await Task.Yield();
            outer.Add(number);
        });

        // Verifies that only one lease is active at a time by ensuring exit order matches enter order.
        enter.ShouldBe(outer);
    }
}