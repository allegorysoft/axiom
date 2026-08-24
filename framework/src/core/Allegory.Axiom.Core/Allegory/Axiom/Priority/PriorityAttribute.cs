using System;
using System.Reflection;

namespace Allegory.Axiom.Priority;

/// <summary>
/// Declares the default <see cref="PriorityLevel"/> for a type, resolved via <see cref="Get(Type)"/>
/// by any module that orders types or their instances (e.g. handlers).
/// Types without this attribute resolve to <see cref="PriorityLevel.Normal"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PriorityAttribute(PriorityLevel level) : Attribute
{
    public PriorityLevel Level { get; } = level;

    public static PriorityLevel Get<T>() => Get(typeof(T));

    public static PriorityLevel Get(Type type)
    {
        return Find(type) ?? PriorityLevel.Normal;
    }

    public static PriorityLevel? Find(Type type)
    {
        var attribute = type.GetCustomAttribute<PriorityAttribute>();
        return attribute?.Level;
    }
}