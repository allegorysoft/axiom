using System;
using System.Diagnostics;
using System.Threading;

namespace Allegory.Axiom.EventBus.Distributed;

public readonly struct EventContext
{
    public Guid Id { get; init; }
    public CancellationToken CancellationToken { get; init; }
    public Activity? Activity { get; init; }
    public IServiceProvider ServiceProvider { get; init; }
}