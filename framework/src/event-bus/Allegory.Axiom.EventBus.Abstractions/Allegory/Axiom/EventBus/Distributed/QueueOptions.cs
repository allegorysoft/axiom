namespace Allegory.Axiom.EventBus.Distributed;

public class QueueOptions
{
    public string? Name { get; set; }
    public QueueTopology Topology { get; set; } = QueueTopology.Single;
}