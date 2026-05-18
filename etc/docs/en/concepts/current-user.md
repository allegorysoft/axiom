---
title: Current User (Principal)
description: Accessing the current security principal in Axiom applications.
---

# Current User (Principal)

Axiom provides `IPrincipalAccessor` for accessing the current `ClaimsPrincipal` in a way that works across both plain .NET contexts and ASP.NET Core.

Two packages are involved:

```bash
dotnet add package Allegory.Axiom.Security.Abstractions
dotnet add package Allegory.Axiom.Security
```

For ASP.NET Core, add the integration package instead of (or alongside) the base package:

```bash
dotnet add package Allegory.Axiom.AspNetCore.Security
```

## `IPrincipalAccessor`

```csharp
public interface IPrincipalAccessor : ISingletonService
{
    ClaimsPrincipal? Current { get; }
}
```

Inject `IPrincipalAccessor` wherever you need the current principal. Returns `null` when no principal is set.

```csharp
public class OrderService(IPrincipalAccessor principal) : IOrderService, ITransientService
{
    public Task<Order> PlaceOrderAsync(Order order)
    {
        var userId = principal.Current?.Identity?.FindNameIdentifier();
        // ...
    }
}
```

## Default Implementation

`ClaimsPrincipalAccessor` is the default implementation. It reads from `ClaimsPrincipal.Current`, which reflects `Thread.CurrentPrincipal` in standard .NET contexts.

It is registered with `RegistrationStrategy.TryAdd`, so providing your own implementation automatically takes precedence.

## ASP.NET Core Integration
 
Adding the `Allegory.Axiom.AspNetCore.Security` package is all that is required. It automatically provides an HTTP-aware implementation that reads the principal from `IHttpContextAccessor.HttpContext.User`, falling back to `ClaimsPrincipal.Current` when no HTTP context is available (for example, in background jobs).

## Custom Accessor

Implement `IPrincipalAccessor` directly for custom scenarios such as test doubles, multi-tenant contexts, or pulling the principal from a custom ambient store.

```csharp
public class MyCustomPrincipalAccessor : IPrincipalAccessor
{
    private readonly IMyContextProvider _context;

    public MyCustomPrincipalAccessor(IMyContextProvider context)
    {
        _context = context;
    }

    public ClaimsPrincipal? Current => _context.CurrentUser;
}
```

Because `ClaimsPrincipalAccessor` is registered with `TryAdd`, any class that implements `IPrincipalAccessor` and is registered before it (or uses `Replace`) will win. The simplest approach is to register your implementation with `RegistrationStrategy.Replace`:

```csharp
[Dependency(Strategy = RegistrationStrategy.Replace)]
public class MyCustomPrincipalAccessor : IPrincipalAccessor { ... }
```