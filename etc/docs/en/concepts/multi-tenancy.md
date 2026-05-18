---
title: Multi-Tenancy
description: Tenant resolution, context management, and access control for multi-tenant Axiom applications.
---

# Multi-Tenancy

::: warning
Once the axiom framework matures, we plan to introduce a tenant management module with database-backed tenants, a UI, and admin APIs. Until then, the current multi-tenancy features are considered "preview" and not ready to be used in production without custom implementation of tenant stores and principal access control.
:::

Multi-tenancy is the ability to serve multiple independent tenants from a single application codebase. Each tenant has isolated data, configuration, and context but shares the same codebase and infrastructure.

Axiom handles this through an **ambient tenant context**: once the current tenant is resolved (from an HTTP header, route, query string, or any custom source), it is stored in an `AsyncLocal` slot and flows automatically through the async call chain. Your services read it without any explicit passing.

Three packages are involved:

```bash
dotnet add package Allegory.Axiom.MultiTenancy.Abstractions
dotnet add package Allegory.Axiom.MultiTenancy
dotnet add package Allegory.Axiom.MultiTenancy.DefaultStore   # optional, for config-based tenants
```

For ASP.NET Core integration:

```bash
dotnet add package Allegory.Axiom.AspNetCore.MultiTenancy
```

## How Axiom Handles Multi-Tenancy

At request time (or at any async boundary), Axiom resolves a `TenantContext` an immutable record holding the tenant's `Id`, `Name`, `NormalizedName`, and any extra metadata and places it in the ambient context. From that point on, any service in the call chain can read the current tenant without knowing how it was resolved.

The flow looks like this:

```
Incoming request
  → ICurrentTenantIdentifierProvider  (extract raw identifier: header / query / route / custom)
  → ITenantStore                      (look up TenantContext by id or name)
  → ICurrentTenantChecker             (verify principal has access)
  → ITenantContextAccessor.Set(tenant) (store in AsyncLocal)
  → your services read ITenantContextAccessor.Current
```

In ASP.NET Core, `MultiTenancyMiddleware` drives this pipeline automatically at the start of each request.

## Accessing the Current Tenant

Inject `ITenantContextAccessor` anywhere in your application to read the ambient tenant:

```csharp
public class OrderService(ITenantContextAccessor accessor) : ITransientService
{
    public Task<IEnumerable<Order>> GetOrdersAsync()
    {
        var tenantId = accessor.Current?.Id
            ?? throw new InvalidOperationException("No tenant context.");

        // Use tenantId to filter data
        return Task.FromResult(Enumerable.Empty<Order>());
    }
}
```

`Current` returns `null` when no tenant context has been set for example, in host-level background jobs or admin operations that do not belong to any tenant. A `null` check is the conventional way to detect host-level execution.

### `ITenantContextAccessor`

```csharp
public interface ITenantContextAccessor
{
    TenantContext? Current { get; }
    void Set(TenantContext? current = null);
    IDisposable Change(TenantContext? current = null);
}
```

`Set` replaces the ambient context for the current async flow. `Change` replaces it temporarily and restores the previous value when the returned `IDisposable` is disposed useful for switching tenants within a background job or integration test:

```csharp
using (accessor.Change(tenantB))
{
    await DoWorkAsync(); // Current = tenantB
}
// Current restored to whatever it was before
```

Nested changes unwind correctly:

```csharp
accessor.Set(tenantA);

using (accessor.Change(tenantB))
{
    // Current = tenantB
    using (accessor.Change(tenantC))
    {
        // Current = tenantC
    }
    // Current = tenantB
}

// Current = tenantA
```

## ASP.NET Core Setup

### Middleware

`MultiTenancyMiddleware` runs the resolution pipeline once per request and sets the ambient tenant. Register it after authentication so principal is available for current tenant checking, and before authorization so the tenant context is available to auth middleware:

```csharp
app.UseRouting();
app.UseAuthentication();
app.UseMultiTenancy();
app.UseAuthorization();
```

If no tenant identifier is found, the middleware does nothing and the request continues as a host-level operation (`Current` remains `null`).

### Tenant Identification

By default, `HttpContextCurrentTenantIdentifierProvider` extracts the tenant identifier from the HTTP request in this priority order:

1. **Header** `Tenant`
2. **Query string** `__tenant`
3. **Route value** `tenant`

Configure the keys:

```csharp
builder.Services.Configure<AspNetCoreMultiTenancyOptions>(options =>
{
    options.HeaderKey = "Tenant";   // default
    options.QueryKey  = "__tenant";   // default
    options.RouteKey  = "tenant";     // default
});
```

For route-based tenants, include the route key in your route template:

```csharp
app.MapControllerRoute(
    name: "tenant",
    pattern: "{tenant}/{controller=Home}/{action=Index}/{id?}");
```

## Tenant Store

`ITenantStore` resolves a `TenantContext` from a raw identifier. Axiom calls it internally during resolution you do not call it directly in most cases.

```csharp
public interface ITenantStore
{
    ValueTask<TenantContext?> FindAsync(Guid id);
    ValueTask<TenantContext?> FindAsync(string name);
}
```

### Default Store

`Allegory.Axiom.MultiTenancy.DefaultStore` provides a configuration-driven implementation. Define tenants in `appsettings.json` under the `Axiom` key:

```json
{
  "Axiom": {
    "Tenants": [
      {
        "id": "11111111-1111-1111-1111-111111111111",
        "name": "acme",
        "normalizedName": "ACME",
        "extraProperties": {
          "plan": "enterprise"
        }
      }
    ],
    "TenantPrincipals": {
      "user-id-here": ["11111111-1111-1111-1111-111111111111"]
    }
  }
}
```

`TenantPrincipals` maps principal IDs (from the `NameIdentifier` claim) to the tenant IDs they may access.

### Custom Store

For database-backed resolution, implement `ITenantStore` directly. Because `NullTenantStore` is registered with `TryAdd`, your implementation takes precedence automatically:

```csharp
public class DatabaseTenantStore(IDbContext db) : ITenantStore
{
    public async ValueTask<TenantContext?> FindAsync(Guid id)
    {
        var row = await db.Tenants.FindAsync(id);
        return row == null ? null : new TenantContext(row.Id, row.Name, row.NormalizedName);
    }

    public async ValueTask<TenantContext?> FindAsync(string name)
    {
        var normalized = name.ToUpperInvariant();
        var row = await db.Tenants.FirstOrDefaultAsync(t => t.NormalizedName == normalized);
        return row == null ? null : new TenantContext(row.Id, row.Name, row.NormalizedName);
    }
}
```

## Principal Access Control

After resolving a tenant from the store, Axiom checks whether the current authenticated principal is allowed to access it. This check runs automatically as part of the resolution pipeline.

`ICurrentTenantChecker` performs the check:

```csharp
public interface ICurrentTenantChecker
{
    Task CheckAsync(TenantContext tenant);
}
```

The default implementation:

1. Reads the current principal via `IPrincipalAccessor`.
2. Skips the check if the identity is `null` or not authenticated (unauthenticated requests are allowed through).
3. Throws if authenticated but no `NameIdentifier` claim is present.
4. Calls `ITenantPrincipalStore.HasAccessAsync` and throws if the principal lacks access.

### Principal Store

```csharp
public interface ITenantPrincipalStore
{
    Task<bool> HasAccessAsync(string principalId, Guid tenantId, CancellationToken cancellationToken = default);
    ValueTask<IReadOnlySet<Guid>> GetTenantListAsync(string principalId, CancellationToken cancellationToken = default);
}
```

`NullTenantPrincipalStore` is registered with `TryAdd` and always denies access. Replace it with your own implementation for production use.

### Custom Checker

Override `CurrentTenantChecker` to extend or replace the access logic:

```csharp
public class MyTenantChecker(
    ITenantPrincipalStore store,
    IPrincipalAccessor accessor) : CurrentTenantChecker(store, accessor)
{
    public override async Task CheckAsync(TenantContext tenant)
    {
        await base.CheckAsync(tenant);
        // additional checks here
    }
}
```

## Tenant Resolution Outside the HTTP Pipeline
 
`ICurrentTenantIdentifierProvider` plugs into the resolution pipeline that `MultiTenancyMiddleware` drives on each HTTP request. For background jobs, message consumers, scheduled tasks, or any code that runs outside a request context, that pipeline never executes there is no middleware to call it.
 
In those cases, resolve the tenant yourself using `ITenantStore` and set it on `ITenantContextAccessor` directly:
 
```csharp
public class OrderProcessingJob(
    ITenantStore tenantStore,
    ITenantContextAccessor tenantAccessor,
    IOrderService orderService) : ITransientService
{
    public async Task ProcessAsync(string tenantName)
    {
        var tenant = await tenantStore.FindAsync(tenantName)
            ?? throw new InvalidOperationException($"Tenant '{tenantName}' not found.");
 
        using (tenantAccessor.Change(tenant))
        {
            // All code here sees the correct tenant via tenantAccessor.Current
            await orderService.ProcessPendingOrdersAsync();
        }
    }
}
```
 
`Change` restores the previous context when disposed, so tenant switches are safe to nest and compose.
 
### Custom Identifier Providers
 
`ICurrentTenantIdentifierProvider` is for extending **how the HTTP pipeline identifies the tenant** for example, resolving it from a custom claim, a domain name, or a subdomain instead of a header or route value:
 
```csharp
public interface ICurrentTenantIdentifierProvider
{
    ValueTask<string?> TryGetAsync();
}
```
 
Return `null` or empty to indicate "no identifier from this source." Multiple providers can be registered; the first non-empty result wins.
 
```csharp
// Resolve tenant from subdomain: acme.myapp.com → "acme"
public class SubdomainTenantIdentifierProvider(IHttpContextAccessor httpContextAccessor)
    : ICurrentTenantIdentifierProvider
{
    public ValueTask<string?> TryGetAsync()
    {
        var host = httpContextAccessor.HttpContext?.Request.Host.Host;
        var subdomain = host?.Split('.').FirstOrDefault();
        return ValueTask.FromResult(subdomain);
    }
}
```

## Tenant Normalization

Names are normalized before lookup via `ITenantNormalizer`. The default uses `ToUpperInvariant()`, which avoids locale-specific issues (e.g., the Turkish dotted-I problem).

```csharp
public interface ITenantNormalizer
{
    string NormalizeName(string name);
}
```

Override to apply custom normalization:

```csharp
public class MyTenantNormalizer : ITenantNormalizer
{
    public string NormalizeName(string name) =>
        name.Trim().ToUpperInvariant().Replace(" ", "-");
}
```

## Tenant-Owned Entities

Implement `ITenantOwned` on entities that belong to a specific tenant:

```csharp
public interface ITenantOwned
{
    Guid? TenantId { get; }
}
```

`TenantId` is nullable to support host-level entities that do not belong to any tenant. Use `ITenantContextAccessor.Current?.Id` to stamp `TenantId` on new entities, and filter queries by it in your data access layer.
