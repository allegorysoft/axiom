using System.Collections.Generic;

namespace Allegory.Axiom.Data;

public class ConnectionStringContextsOptions
{
    public HashSet<ConnectionStringContextOptions> Contexts { get; } = [];
}

public class ConnectionStringContextOptions
{
    public string Name { get; init; } = null!;
    public HashSet<string> Connections { get; init; } = [];
    public bool IsTenantAgnostic { get; init; }

    public override bool Equals(object? obj) => obj is ConnectionStringContextOptions options && options.Name == Name;

    public override int GetHashCode() => Name.GetHashCode();
}