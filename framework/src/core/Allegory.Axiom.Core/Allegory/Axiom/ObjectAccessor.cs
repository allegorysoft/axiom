using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Allegory.Axiom;

public static class ObjectAccessor
{
    private static readonly ConcurrentDictionary<
        Type, ConcurrentDictionary<string, PropertyInfo?>> PropertyCache = new();

    public static void TrySetProperty<TObject, TValue>(
        TObject obj,
        Expression<Func<TObject, TValue>> selector,
        TValue value)
    {
        var propertyName = GetPropertyName(selector);
        if (propertyName is null)
        {
            return;
        }

        TrySetProperty(obj, propertyName, value);
    }

    public static void TrySetProperty<TObject, TValue>(
        TObject obj,
        string propertyName,
        TValue value)
    {
        if (obj is null)
        {
            return;
        }

        var property = GetOrAddPropertyInfo(obj.GetType(), propertyName);
        property?.SetValue(obj, value);
    }

    public static void TrySetProperty<TObject, TValue>(
        TObject obj,
        Expression<Func<TObject, TValue>> selector,
        Func<TValue> factory)
    {
        var propertyName = GetPropertyName(selector);
        if (propertyName is null)
        {
            return;
        }

        TrySetProperty(obj, propertyName, factory);
    }

    public static void TrySetProperty<TObject, TValue>(
        TObject obj,
        string propertyName,
        Func<TValue> factory)
    {
        if (obj is null)
        {
            return;
        }

        var property = GetOrAddPropertyInfo(obj.GetType(), propertyName);
        property?.SetValue(obj, factory());
    }

    private static PropertyInfo? GetOrAddPropertyInfo(Type objectType, string propertyName)
    {
        var typeCache =
            PropertyCache.GetOrAdd(objectType, static _ => new ConcurrentDictionary<string, PropertyInfo?>());
        return typeCache.GetOrAdd(propertyName, static (name, type) => GetPropertyInfo(type, name), objectType);
    }

    private static string? GetPropertyName<TObject, TValue>(Expression<Func<TObject, TValue>> selector)
    {
        var memberExpression = selector.Body switch
        {
            MemberExpression member => member,
            UnaryExpression {NodeType: ExpressionType.Convert} unary => unary.Operand as MemberExpression,
            _ => null
        };
        return memberExpression?.Member.Name;
    }

    private static PropertyInfo? GetPropertyInfo(Type objectType, string propertyName)
    {
        var propertyInfo = objectType.GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (propertyInfo is null)
        {
            return null;
        }

        var setMethod = propertyInfo.GetSetMethod(nonPublic: true);
        if (setMethod is null)
        {
            return null;
        }

        return propertyInfo;
    }
}