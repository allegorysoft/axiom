using System;
using System.Reflection;

namespace Allegory.Axiom.Localization;

[AttributeUsage(AttributeTargets.Class)]
public class ResourceNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;

    public static string Get<T>()
    {
        return Get(typeof(T));
    }

    public static string Get(Type type)
    {
        var attribute = type.GetCustomAttribute<ResourceNameAttribute>();

        if (attribute == null)
        {
            return type.FullName 
                   ?? throw new ArgumentException("Resource name cannot be null");
        }

        return attribute.Name;
    }
}