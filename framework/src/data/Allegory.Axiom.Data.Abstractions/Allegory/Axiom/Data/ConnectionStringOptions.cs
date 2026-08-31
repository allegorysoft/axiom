using System.Collections.Generic;

namespace Allegory.Axiom.Data;

public class ConnectionStringContextsOptions
{
    public HashSet<ConnectionStringContextOptions> Contexts { get; set; } = null!;
}

public class ConnectionStringContextOptions
{
    public string Name { get; init; } = null!;
    public HashSet<string> Connections { get; set; } = [];
    public bool IsTenantAgnostic { get; set; }

    public override bool Equals(object? obj) => obj is ConnectionStringContextOptions options && options.Name == Name;

    public override int GetHashCode() => Name.GetHashCode();
}