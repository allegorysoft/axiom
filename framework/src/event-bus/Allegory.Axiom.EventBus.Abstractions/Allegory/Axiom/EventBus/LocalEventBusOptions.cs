using System;
using System.Collections.Generic;

namespace Allegory.Axiom.EventBus;

public class LocalEventBusOptions
{
    public required Dictionary<Type, List<Type>> Handlers { get; set; }
}