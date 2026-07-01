using System;
using System.Reflection;

namespace Allegory.Axiom.EventBus;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TopicNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;

    public static string Get<T>()
    {
        return Get(typeof(T));
    }

    public static string Get(Type type)
    {
        var attribute = type.GetCustomAttribute<TopicNameAttribute>();

        if (attribute == null)
        {
            return type.FullName
                   ?? throw new ArgumentException("Event name cannot be null");
        }

        return attribute.Name;
    }
}