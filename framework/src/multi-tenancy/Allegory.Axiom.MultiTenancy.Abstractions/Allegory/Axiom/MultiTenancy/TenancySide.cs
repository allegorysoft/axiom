using System;

namespace Allegory.Axiom.MultiTenancy;

[Flags]
public enum TenancySide : byte
{
    Tenant = 1,
    Host = 2,
    Both = Tenant | Host
}