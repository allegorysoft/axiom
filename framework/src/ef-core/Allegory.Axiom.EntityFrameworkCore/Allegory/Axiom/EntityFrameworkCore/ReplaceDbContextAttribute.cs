using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ReplaceDbContextAttribute : Attribute
{
    public IReadOnlyList<Type> ReplacedContexts { get; }

    public ReplaceDbContextAttribute(params Type[] replacedContexts)
    {
        ArgumentNullException.ThrowIfNull(replacedContexts);

        if (replacedContexts.Length == 0)
        {
            throw new ArgumentException(
                "At least one replaced DbContext type must be specified.",
                nameof(replacedContexts));
        }

        foreach (var type in replacedContexts)
        {
            if (!typeof(DbContext).IsAssignableFrom(type))
            {
                throw new ArgumentException(
                    $"Type '{type.Name}' does not derive from {nameof(DbContext)}.",
                    nameof(replacedContexts));
            }
        }

        ReplacedContexts = replacedContexts;
    }

    public static IReadOnlyList<Type> Get(Type type)
    {
        return Find(type) ?? throw new ArgumentException("Replace db context not found.", nameof(type));
    }

    public static IReadOnlyList<Type>? Find(Type type)
    {
        var attribute = type.GetCustomAttribute<ReplaceDbContextAttribute>();

        return attribute?.ReplacedContexts;
    }
}