using System;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public static class DbContextExtensions
{
    extension(DbContext context)
    {
        internal Guid? GetCurrentTenantId() => ITenantContextAccessor.TryGetCurrent()?.Id;
    }
}