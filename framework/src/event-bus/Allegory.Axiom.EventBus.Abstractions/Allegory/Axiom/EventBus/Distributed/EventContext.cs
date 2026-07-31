using System;
using System.Diagnostics;

namespace Allegory.Axiom.EventBus.Distributed;

public readonly struct EventContext
{
    public Guid Id { get; init; }
    public Activity? Activity { get; init; }
}