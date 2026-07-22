using System;
using System.Reflection;

namespace Allegory.Axiom.Caching;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class CacheNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;

    public static string Get<T>()
    {
        return Get(typeof(T));
    }

    public static string Get(Type type)
    {
        var attribute = type.GetCustomAttribute<CacheNameAttribute>();

        if (attribute == null)
        {
            return type.FullName ?? throw new ArgumentException("Cache name cannot be null");
        }

        return attribute.Name;
    }
}