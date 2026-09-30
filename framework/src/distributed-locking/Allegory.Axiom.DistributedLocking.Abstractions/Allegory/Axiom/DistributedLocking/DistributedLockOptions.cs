using System.Collections.Generic;
using Allegory.Axiom.Extensibility;

namespace Allegory.Axiom.DistributedLocking;

public class DistributedLockOptions : IExtraProperties
{
    public const string Section = "Axiom:DistributedLock";

    public string? KeyPrefix { get; set; }
    public IDictionary<string, object> ExtraProperties { get; } = new Dictionary<string, object>();
}