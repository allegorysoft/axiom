using System;
using System.Reflection;

namespace Allegory.Axiom.MultiTenancy;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TenancySideAttribute(TenancySide side) : Attribute
{
    public TenancySide Side { get; } = side;

    public static TenancySide Get(Type type)
    {
        return Find(type) ?? throw new ArgumentException("Tenancy side cannot be null");
    }

    public static TenancySide? Find(Type type)
    {
        var attribute = type.GetCustomAttribute<TenancySideAttribute>();

        return attribute?.Side;
    }
}