using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.RabbitMQ;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Benchmarks.Integration.RabbitMQ;

[MemoryDiagnoser]
[SimpleJob(3, 3, 3, 3)]
public class RabbitMqClientFactoryBenchmark
{
    private RabbitMqClientFactory _factory = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        var builder = Host.CreateApplicationBuilder();
        await builder.ConfigureApplicationAsync();
        builder.Services.Configure<RabbitMqOptions>(o =>
        {
            o[RabbitMqOptions.DefaultConnectionName] = new RabbitMqOption
            {
                Hostname = "localhost",
                Username = "guest",
                Password = "guest"
            };
        });

        var host = builder.Build();
        _factory = host.Services.GetRequiredService<RabbitMqClientFactory>();
        await _factory.GetAsync(RabbitMqOptions.DefaultConnectionName);
    }

    [GlobalCleanup]
    public async Task CleanupAsync()
    {
        await _factory.DisposeAsync();
    }

    [Benchmark]
    public async Task GetConnectionAsync()
    {
        //Shouldn't allocate heap memory 
        await _factory.GetAsync(RabbitMqOptions.DefaultConnectionName);
    }

    [Benchmark]
    public async Task GetChannelAsync()
    {
        //Shouldn't allocate heap memory 
        var client = await _factory.GetAsync(RabbitMqOptions.DefaultConnectionName);
        var channel = await client.GetChannelAsync("c-1");
    }
}