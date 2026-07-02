using System;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace Allegory.Axiom.EventBus;

internal sealed class EventBusRabbitMqTestsPackage : IPostConfigureApplication
{
    public static async Task PostConfigureAsync(IHostApplicationBuilder builder)
    {
        // var container = new RabbitMqBuilder("rabbitmq:latest")
        //     .WithUsername("guest")
        //     .WithPassword("guest")
        //     .Build();
        //
        // await builder.AddTestContainerAsync(container);
        //
        // builder.Services.Configure<RabbitMqOptions>(o =>
        // {
        //     var option = new RabbitMqOption
        //     {
        //         Factory = _ =>
        //         {
        //             var connectionFactory = new ConnectionFactory
        //             {
        //                 Uri = new Uri(container.GetConnectionString())
        //             };
        //
        //             return connectionFactory.CreateConnectionAsync();
        //         }
        //     };
        //
        //     o[RabbitMqOptions.DefaultConnectionName] = option;
        // });

        builder.Services.Configure<RabbitMqOptions>(o =>
        {
            var option = new RabbitMqOption
            {
                Hostname = "localhost",
                Username = "guest",
                Password = "guest",
            };
        
            o[RabbitMqOptions.DefaultConnectionName] = option;
        });

        builder.Services.Configure<DistributedEventBusOptions>(options =>
        {
            options.RabbitMq.ExchangeName = "app-pool-1";
        });
    }
}