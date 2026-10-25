using System;
using System.Collections.Generic;
using Allegory.Axiom.Domain.Entities;

namespace Allegory.Axiom.Localization;

public class LocalizationRecord : AggregateRoot<Guid>
{
    public const int MaxNameLength = 128;
    public const int MaxCultureLength = 20;

    public LocalizationRecord(string name, string culture, bool isDefault = false)
    {
        SetName(name);
        SetCulture(culture);
        IsDefault = isDefault;
    }

    public string Name { get; protected set; } = null!;
    public string Culture { get; protected set; } = null!;
    public bool IsDefault { get; set; }
    public Dictionary<string, string> Translations { get; set; } = [];

    public void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, MaxNameLength);

        Name = name;
    }

    public void SetCulture(string culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(culture);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(culture.Length, MaxCultureLength);

        Culture = culture;
    }
}