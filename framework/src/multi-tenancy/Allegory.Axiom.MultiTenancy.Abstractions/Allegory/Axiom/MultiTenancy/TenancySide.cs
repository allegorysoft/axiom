using System;

namespace Allegory.Axiom.MultiTenancy;

[Flags]
public enum TenancySide : byte
{
    Host = 1,
    Tenant = 2,
    Hybrid =  Host | Tenant
}