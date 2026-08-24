namespace Allegory.Axiom.Priority;

/// <summary>
/// A general-purpose priority level for ordering handlers, hooks, or actions
/// across anything. Lower values run first.
/// Gaps between named values are intentional cast an arbitrary byte
/// (e.g. <c>(PriorityLevel)75</c>) to slot between two named levels.
/// </summary>
public enum PriorityLevel : byte
{
    Highest = 50,
    High = 100,
    Normal = 150,
    Low = 200,
    Lowest = 250,
}