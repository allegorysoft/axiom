using System;
using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace Allegory.Axiom.EventBus;

internal sealed class EventBusRabbitMqTestsPackage : IConfigureApplication
{
    public static async Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        var container = new RabbitMqBuilder("rabbitmq:latest")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();
        
        await builder.AddTestContainerAsync(container);
        
        builder.Services.Configure<RabbitMqOptions>(o =>
        {
            var option = new RabbitMqOption
            {
                Factory = _ =>
                {
                    var connectionFactory = new ConnectionFactory
                    {
                        Uri = new Uri(container.GetConnectionString())
                    };
        
                    return connectionFactory.CreateConnectionAsync();
                }
            };
        
            o[RabbitMqOptions.DefaultConnectionName] = option;
        });

        // builder.Services.Configure<RabbitMqOptions>(o =>
        // {
        //     var option = new RabbitMqOption
        //     {
        //         Hostname = "localhost",
        //         Username = "guest",
        //         Password = "guest",
        //     };
        //
        //     o[RabbitMqOptions.DefaultConnectionName] = option;
        // });
        //

        builder.Services.Configure<RabbitMqEventBusOptions>(o =>
        {
            o.ExchangeName = "app-1";
        });
    }
}