using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqOption
{
    public Func<RabbitMqOption, Task<IConnection>>? Factory { get; set; }

    public string? Hostname { get; set; }
    public IEnumerable<string>? Hostnames { get; set; }
    public int Port { get; set; } = 5672;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string VirtualHost { get; set; } = "/";
    public string? ClientProvidedName { get; set; }
}