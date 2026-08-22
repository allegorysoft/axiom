using System;

namespace Allegory.Axiom.Priority;

/// <summary>
/// Combines a <see cref="PriorityLevel"/> with a registration sequence to give
/// stable, FIFO-within-priority ordering when used as the priority type of
/// <see cref="System.Collections.Generic.PriorityQueue{TElement,TPriority}"/>.
/// </summary>
public readonly record struct PrioritySortOrder(PriorityLevel Priority, ushort Sequence) : IComparable<PrioritySortOrder>
{
    public int CompareTo(PrioritySortOrder other) =>
        Priority != other.Priority
            ? Priority.CompareTo(other.Priority)
            : Sequence.CompareTo(other.Sequence);
}