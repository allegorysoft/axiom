using System;
using System.Collections.Generic;

namespace Allegory.Axiom.Data.Filtering;

public class FilterSwitchOptions
{
    public Dictionary<Type, bool> Defaults { get; } = [];
}