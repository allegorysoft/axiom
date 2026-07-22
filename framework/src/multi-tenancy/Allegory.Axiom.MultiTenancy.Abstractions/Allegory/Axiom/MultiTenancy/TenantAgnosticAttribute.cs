using System;

namespace Allegory.Axiom.MultiTenancy;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
    Inherited = false)]
public sealed class TenantAgnosticAttribute : Attribute {}