using System;
using System.Reflection;

namespace Allegory.Axiom.EventBus;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class EventOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;

    public static int Get<T>()
    {
        return Get(typeof(T));
    }

    public static int Get(Type type)
    {
        var attribute = type.GetCustomAttribute<EventOrderAttribute>();

        return attribute?.Order ?? 0;
    }
}