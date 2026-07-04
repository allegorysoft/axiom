using System;
using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace Allegory.Axiom.RabbitMQ;

internal sealed class RabbitMqTestsPackage : IConfigureApplication
{
    public const string SecondConnectionName = "second";

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

            // Multiple connections can connect to the same RabbitMQ server.
            // Each connection creates its own TCP connection.
            o[SecondConnectionName] = option;
        });
    }
}