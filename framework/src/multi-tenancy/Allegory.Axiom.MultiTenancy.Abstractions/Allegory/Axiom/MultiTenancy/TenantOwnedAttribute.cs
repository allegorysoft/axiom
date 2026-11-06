using System;

namespace Allegory.Axiom.MultiTenancy;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class TenantOwnedAttribute : Attribute;