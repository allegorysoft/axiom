using System;
using Allegory.Axiom.Domain.Entities;

namespace Allegory.Axiom.Setting;

public class SettingValueRecord : AggregateRoot<Guid>
{
    public static int MaxNameLength { get; set; } = 128;
    public static int MaxContributorNameLength { get; set; } = 64;
    public static int MaxContributorKeyLength { get; set; } = 64;
    public static int MaxValueLength { get; set; } = 4_000;

    protected SettingValueRecord() { }

    public SettingValueRecord(
        string name,
        string contributorName,
        string? contributorKey,
        string value)
    {
        SetName(name);
        SetContributorName(contributorName);
        SetValue(value);

        if (contributorKey is not null)
        {
            SetContributorKey(contributorKey);
        }
    }

    public string Name { get; protected set; } = null!;
    public string ContributorName { get; protected set; } = null!;
    public string? ContributorKey { get; protected set; }
    public string Value { get; protected set; } = null!;

    public void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, MaxNameLength);

        Name = name;
    }

    public void SetContributorName(string contributorName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contributorName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(contributorName.Length, MaxContributorNameLength);

        ContributorName = contributorName;
    }

    public void SetContributorKey(string? contributorKey)
    {
        if (contributorKey is not null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(contributorKey.Length, MaxContributorKeyLength);
        }

        ContributorKey = contributorKey;
    }

    public void SetValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, MaxValueLength);

        Value = value;
    }
}