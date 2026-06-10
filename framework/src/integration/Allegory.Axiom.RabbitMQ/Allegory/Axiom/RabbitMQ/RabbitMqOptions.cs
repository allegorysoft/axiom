using System.Collections.Generic;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqOptions : Dictionary<string, RabbitMqOption>
{
    public const string DefaultConnectionName = "Default";
}