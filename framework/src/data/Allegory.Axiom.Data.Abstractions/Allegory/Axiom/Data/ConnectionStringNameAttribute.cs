using System;
using System.Reflection;

namespace Allegory.Axiom.Data;

[AttributeUsage(AttributeTargets.Class)]
public class ConnectionStringNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;

    public static string Get(Type type)
    {
        return Find(type) ?? throw new ArgumentException("Connection string name cannot be null");
    }

    public static string? Find(Type type)
    {
        var attribute = type.GetCustomAttribute<ConnectionStringNameAttribute>();

        return attribute?.Name;
    }

}