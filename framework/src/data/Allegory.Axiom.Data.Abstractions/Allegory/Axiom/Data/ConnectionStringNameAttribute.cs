using System;
using System.Reflection;

namespace Allegory.Axiom.Data;

[AttributeUsage(AttributeTargets.Class)]
public class ConnectionStringNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;

    public static string Get<T>()
    {
        return Get(typeof(T));
    }

    public static string Get(Type type)
    {
        var attribute = type.GetCustomAttribute<ConnectionStringNameAttribute>();

        if (attribute == null)
        {
            return type.FullName ?? throw new ArgumentException("Connection string name cannot be null");
        }

        return attribute.Name;
    }

    public static string? TryGet(Type type)
    {
        var attribute = type.GetCustomAttribute<ConnectionStringNameAttribute>();

        return attribute?.Name;
    }
}