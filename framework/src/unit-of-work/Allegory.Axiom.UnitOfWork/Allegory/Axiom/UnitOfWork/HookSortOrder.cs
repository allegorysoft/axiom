using System;

namespace Allegory.Axiom.UnitOfWork;

internal readonly record struct HookSortOrder(
    UnitOfWorkHookPriority Priority,
    ushort Sequence)
    : IComparable<HookSortOrder>
{
    public int CompareTo(HookSortOrder other) =>
        Priority != other.Priority
            ? Priority.CompareTo(other.Priority)
            : Sequence.CompareTo(other.Sequence);
}