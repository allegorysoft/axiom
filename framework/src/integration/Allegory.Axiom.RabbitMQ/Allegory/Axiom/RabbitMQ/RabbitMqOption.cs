using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqOption
{
    public Func<Task<IConnection>>? Factory { get; set; }

    public string? Hostname { get; set; }
    public IEnumerable<string>? Hostnames { get; set; }
    public int Port { get; set; } = 5672;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string VirtualHost { get; set; } = "/";
    public string? ClientProvidedName { get; set; }

    public async Task<IConnection> DefaultFactory()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Hostname);
        ArgumentException.ThrowIfNullOrWhiteSpace(Username);
        ArgumentException.ThrowIfNullOrWhiteSpace(Password);

        var factory = new ConnectionFactory
        {
            HostName = Hostname,
            UserName = Username,
            Password = Password,
            VirtualHost = VirtualHost,
            ClientProvidedName = ClientProvidedName ?? Assembly.GetEntryAssembly()?.FullName
        };

        return await factory.CreateConnectionAsync();
    }
}