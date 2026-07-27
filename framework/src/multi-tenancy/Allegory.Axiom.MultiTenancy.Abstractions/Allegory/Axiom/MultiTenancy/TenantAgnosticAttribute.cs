using System;

namespace Allegory.Axiom.MultiTenancy;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct,
    Inherited = false)]
public sealed class TenantAgnosticAttribute : Attribute {}