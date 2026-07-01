namespace Allegory.Axiom.EventBus.Distributed;

public class QueueOptions
{
    public string? Name { get; set; }// Based on topology, use for Prefix or Queue name
    public QueueTopology Topology { get; set; } = QueueTopology.Single;
}