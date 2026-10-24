using System;
using System.Collections.Generic;
using Allegory.Axiom.Domain.Entities;

namespace Allegory.Axiom.Localization;

public class LocalizationRecord : AggregateRoot<Guid>
{
    public string Name { get; protected set; } = null!;
    public string Culture { get; protected set; } = null!; 
    public bool IsDefault { get; set; }
    public Dictionary<string, string> Translations { get; set; } = [];
}