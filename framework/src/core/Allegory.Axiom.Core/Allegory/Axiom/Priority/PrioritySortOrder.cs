using System;

namespace Allegory.Axiom.Priority;

/// <summary>
/// Combines a <see cref="PriorityLevel"/> with a registration sequence to give
/// stable, FIFO-within-priority ordering when used as the priority type of
/// <see cref="System.Collections.Generic.PriorityQueue{TElement,TPriority}"/>.
/// </summary>
public readonly record struct PrioritySortOrder<TSequence>(
    PriorityLevel Priority, TSequence Sequence)
    : IComparable<PrioritySortOrder<TSequence>>
    where TSequence : IComparable<TSequence>
{
    public int CompareTo(PrioritySortOrder<TSequence> other) =>
        Priority != other.Priority
            ? Priority.CompareTo(other.Priority)
            : Sequence.CompareTo(other.Sequence);
}