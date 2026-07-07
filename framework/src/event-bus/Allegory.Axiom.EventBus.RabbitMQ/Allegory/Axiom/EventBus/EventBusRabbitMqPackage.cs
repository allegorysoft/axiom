using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.EventBus;

internal sealed class EventBusRabbitMqPackage : IConfigureApplication
{
    internal const string RabbitMqOptionsKey = "RabbitMQ";

    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<DistributedEventBusOptions>(options =>
        {
            builder.Configuration
                .GetSection("Axiom:EventBus:Distributed:" + RabbitMqOptionsKey)
                .Bind(options.RabbitMq);
        });

        return Task.CompletedTask;
    }
}