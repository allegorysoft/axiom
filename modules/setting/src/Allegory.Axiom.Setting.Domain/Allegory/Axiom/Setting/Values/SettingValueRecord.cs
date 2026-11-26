using System;
using Allegory.Axiom.Domain.Entities;

namespace Allegory.Axiom.Setting;

public class SettingValueRecord : AggregateRoot<Guid>
{
    public static int MaxNameLength { get; set; } = 128;
    public static int MaxIdentifierNameLength { get; set; } = 64;
    public static int MaxIdentifierKeyLength { get; set; } = 64;
    public static int MaxValueLength { get; set; } = 4_000;

    protected SettingValueRecord() { }

    public SettingValueRecord(
        string name,
        SettingValueIdentifier identifier,
        string value)
    {
        SetName(name);
        SetIdentifier(identifier);
        SetValue(value);
    }

    public string Name { get; protected set; } = null!;
    public SettingValueIdentifier Identifier { get; protected set; }
    public string Value { get; protected set; } = null!;

    public void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, MaxNameLength);

        Name = name;
    }

    protected void SetIdentifier(SettingValueIdentifier identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier.Name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(identifier.Name.Length, MaxIdentifierNameLength);

        if (identifier.Key is not null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(identifier.Key.Length, MaxIdentifierKeyLength);
        }

        Identifier = identifier;
    }

    public void SetValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, MaxValueLength);

        Value = value;
    }
}