using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqClientTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected async Task<RabbitMqClient> GetClientAsync()
        => await fixture.Service<RabbitMqClientFactory>()
            .GetAsync(RabbitMqOptions.DefaultConnectionName);

    [Fact]
    public async Task ShouldGetChannel()
    {
        var client = await GetClientAsync();
        var channel = await client.GetChannelAsync("ch-open");

        channel.ShouldNotBeNull();
        channel.Channel.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnSameChannelInstanceForSameName()
    {
        var client = await GetClientAsync();

        var first = await client.GetChannelAsync("ch-cache");
        var second = await client.GetChannelAsync("ch-cache");

        second.ShouldBeSameAs(first);
    }

    [Fact]
    public async Task ShouldReturnDistinctChannelsForDistinctNames()
    {
        var client = await GetClientAsync();

        var ch1 = await client.GetChannelAsync("ch-distinct-1");
        var ch2 = await client.GetChannelAsync("ch-distinct-2");

        ch2.ShouldNotBe(ch1);
    }

    [Fact]
    public async Task ShouldHandleConcurrentGetChannelAsyncCallsSafely()
    {
        var client = await GetClientAsync();
        var results = new RabbitMqChannel[8];

        await Parallel.ForAsync(0, 8, async (i, _) =>
        {
            results[i] = await client.GetChannelAsync("ch-concurrent");
        });

        for (var i = 1; i < 8; i++)
        {
            results[i].ShouldBeSameAs(results[0]);
        }
    }

    [Fact]
    public async Task ShouldRentChannel()
    {
        var client = await GetClientAsync();

        using var lease = await client.RentChannelAsync("ch-rent");

        lease.Channel.ShouldNotBeNull();
        lease.Channel.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldAllowOnlyOneActiveLeasePerChannelName()
    {
        var client = await GetClientAsync();
        var enter = new List<int>();
        var outer = new List<int>();

        var work = async () =>
        {
            var number = Random.Shared.Next();
            using var lease = await client.RentChannelAsync("ch-exclusive");
            enter.Add(number);
            await Task.Yield();
            outer.Add(number);
        };

        var tasks = new List<Task>();

        for (var i = 0; i < 8; i++)
        {
            tasks.Add(work());
        }

        await Task.WhenAll(tasks);

        // Verifies that only one lease is active at a time by ensuring exit order matches enter order.
        enter.ShouldBe(outer);
    }
}