using System;
using System.Collections.Generic;
using Allegory.Axiom.RabbitMQ;

namespace Allegory.Axiom.EventBus;

public class RabbitMqEventBusOptions
{
    public string ConnectionName { get; set; } = RabbitMqOptions.DefaultConnectionName;
    public string? ExchangeName { get; set; }
    public RabbitMqEventBusQueueOptions Queue { get; set; } = new();
}

public class RabbitMqEventBusQueueOptions
{
    public RabbitMqEventBusQueueOption Default { get; set; } = new();
    public Dictionary<string, RabbitMqEventBusQueueOption> PerQueue { get; set; } = [];

    public RabbitMqEventBusQueueOption Get(string queueName)
    {
        return PerQueue.TryGetValue(queueName, out var queueOptions) ? queueOptions : Default;
    }
}

public class RabbitMqEventBusQueueOption
{
    public uint PrefetchSize { get; set; }
    public ushort PrefetchCount { get; set; } = (ushort) Environment.ProcessorCount;
    public bool Global { get; set; }
}