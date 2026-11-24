using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Allegory.Axiom.Setting;

public class ConfigurationSettingValueContributor(IConfiguration configuration) : ISettingValueContributor
{
    public const string Section = "Axiom:Settings:";

    public string Name => ISettingValueContributor.Configuration;
    protected IConfiguration Configuration { get; } = configuration;
    protected ConcurrentDictionary<SettingDefinition, SettingValue?> Values { get; } = new();

    public virtual ValueTask<SettingValue?> FindAsync(SettingDefinition definition)
    {
        var value = Values.GetOrAdd(
            definition,
            static (d, instance) =>
            {
                var result = instance.GetJsonElement($"{Section}{d.Name}");
                return result.HasValue ? new SettingValue(result.Value) : null;
            },
            this);

        return new ValueTask<SettingValue?>(value);
    }

    protected virtual JsonElement? GetJsonElement(string key)
    {
        var section = Configuration.GetSection(key);

        if (!section.Exists())
        {
            return null;
        }

        return JsonSerializer.SerializeToElement(ConvertSection(section));
    }

    protected virtual object? ConvertSection(IConfigurationSection section)
    {
        var children = section.GetChildren().ToArray();

        if (children.Length == 0)
        {
            return ParsePrimitive(section.Value);
        }

        if (IsSequentialArray(children, out var orderedChildren))
        {
            return orderedChildren
                .Select(ConvertSection)
                .ToArray();
        }

        return children.ToDictionary(
            x => x.Key,
            ConvertSection);
    }

    protected virtual object? ParsePrimitive(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (bool.TryParse(value, out var boolResult))
        {
            return boolResult;
        }

        return value;
    }

    private static bool IsSequentialArray(
        IConfigurationSection[] children,
        out IConfigurationSection[] orderedChildren)
    {
        var indexed = new (int Index, IConfigurationSection Section)[children.Length];

        for (var i = 0; i < children.Length; i++)
        {
            if (!int.TryParse(children[i].Key, out var index) || index < 0)
            {
                orderedChildren = [];
                return false;
            }

            indexed[i] = (index, children[i]);
        }

        Array.Sort(indexed, static (a, b) => a.Index.CompareTo(b.Index));

        for (var i = 0; i < indexed.Length; i++)
        {
            if (indexed[i].Index != i)
            {
                orderedChildren = [];
                return false;
            }
        }

        orderedChildren = indexed.Select(x => x.Section).ToArray();
        return true;
    }
}