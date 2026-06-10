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
    public static async Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        // var container = new RabbitMqBuilder("rabbitmq:latest")
        //     .WithUsername("guest")
        //     .WithPassword("guest")
        //     .Build();
        //
        // await builder.AddTestContainerAsync(container);
        //
        // builder.Services.PostConfigure<RabbitMqOptions>(o =>
        // {
        //     o[RabbitMqOptions.DefaultConnectionName].Factory = _ =>
        //     {
        //         var connectionFactory = new ConnectionFactory
        //         {
        //             Uri = new Uri(container.GetConnectionString())
        //         };
        //
        //         return connectionFactory.CreateConnectionAsync();
        //     };
        // });
    }
}