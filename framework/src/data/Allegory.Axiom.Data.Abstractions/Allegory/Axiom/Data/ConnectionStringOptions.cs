using System.Collections.Generic;

namespace Allegory.Axiom.Data;

public class ConnectionStringOptions
{
    public string DefaultName { get; set; } = "Default";

    public HashSet<ConnectionStringContextOptions>? Contexts { get; set; }
}

public class ConnectionStringContextOptions
{
    public required string Name { get; init; } = null!;
    public required HashSet<string> Connections { get; set; } = [];
    public bool IsTenantAgnostic { get; set; }

    public override bool Equals(object? obj) => obj is ConnectionStringContextOptions options && options.Name == Name;

    public override int GetHashCode() => Name.GetHashCode();
}