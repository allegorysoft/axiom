using System;
using RabbitMQ.Client;

namespace Allegory.Axiom.RabbitMQ;

public readonly struct RabbitMqChannelLease(RabbitMqChannel channel) : IDisposable
{
    public RabbitMqChannel RabbitMqChannel => channel;
    public IChannel Channel => channel.Channel;
    public void Dispose() => channel.Semaphore.Release();
}