---
title: Unit of Work
description: Transactional boundary management with automatic interception support for .NET applications.
---

# Unit of Work

A unit of work groups multiple database operations into a single transactional boundary. Without it, each operation commits independently, which means a failure halfway through leaves your data in a partially updated state. The unit of work pattern solves this by deferring all commits until the entire operation succeeds, then rolling back everything if any part fails.

Axiom's implementation manages the ambient transaction context through `AsyncLocal`, so unit of work nest naturally across service boundaries without passing anything explicitly. Database integrations plug in via `UnitOfWorkDatabaseHandle`, and the interception pipeline can open and close unit of work automatically based on attributes or marker interfaces.

Two packages are involved:

```bash
dotnet add package Allegory.Axiom.UnitOfWork.Abstractions
dotnet add package Allegory.Axiom.UnitOfWork
```

`Allegory.Axiom.UnitOfWork.Abstractions` provides the core interfaces, options, and the `[UnitOfWork]` attribute. `Allegory.Axiom.UnitOfWork` provides the implementations, the manager, and the interceptor.

::: tip
In most cases you do not need to manage unit of work directly. When using a host integration such as ASP.NET Core, a middleware already opens a unit of work for each incoming request and completes or rolls it back automatically. Manual usage via `IUnitOfWorkManager` is only needed for background jobs, custom pipelines, or cases where you need explicit control over transaction boundaries.
:::

## Default Options

Configure default transaction options through the options pattern. These apply whenever a unit of work is begun without explicit options.

```csharp
builder.Services.Configure<UnitOfWorkOptions>(options =>
{
    options.Timeout = TimeSpan.FromSeconds(30);
    options.IsolationLevel = IsolationLevel.ReadCommitted;
});
```

| Property | Type | Default | Description |
|---|---|---|---|
| `TransactionBehavior` | `UnitOfWorkTransactionBehavior` | `Required` | Default transaction behavior when none is specified per call. |
| `IsolationLevel` | `IsolationLevel?` | `null` | Default isolation level passed to the database transaction. |
| `Timeout` | `TimeSpan?` | `null` | Default timeout for unit of work operations. |

When `Begin` is called with options that contain `null` values, those values fall back to defaults.

```csharp
// Default: Timeout = 30s, IsolationLevel = ReadCommitted
var opts = new UnitOfWorkOptions(isolationLevel: IsolationLevel.ReadUncommitted);

await using var uow = manager.Begin(opts);

// Result:
// uow.Options.IsolationLevel = ReadUncommitted (explicit value)
// uow.Options.Timeout = 30s (default fallback)
```

**Rules:**

* Only `null` properties fall back to defaults
* Explicit (non-null) values always take precedence

## `IUnitOfWorkManager`

`IUnitOfWorkManager` is the entry point for managing unit of work boundaries manually. It is registered as a singleton automatically.

```csharp
public interface IUnitOfWorkManager : ISingletonService
{
    IUnitOfWork? Current { get; }
    IUnitOfWork Begin(UnitOfWorkOptions? options = null);
}
```

`Current` returns the ambient unit of work for the current async context, or `null` if none is active.

`Begin` creates and returns a new unit of work. If no unit of work is currently active, it always creates a fresh root. If one is already active, the result depends on the requested `TransactionBehavior` it either joins the existing one as a child or starts a new independent root alongside it.

| Active (Parent) UoW | `Required` | `RequiresNew` | `Suppress` |
|---|---|---|---|
| None | → Root | → Root | → Root |
| `Required` or `RequiresNew` | ↳ Child | → Root | → Root |
| `Suppress` | → Root | → Root | ↳ Child |

- **→ Root** starts an independent transaction. The previous ambient unit of work (if any) is stored as `Parent` and restored when this one is disposed.
- **↳ Child** delegates all operations to the active unit of work. Calling `CompleteAsync` on a child is a no-op; only the root commits.

When `Begin` is called with `null` options, the registered `UnitOfWorkOptions` defaults are used.

Always dispose the unit of work with `await using` so the ambient context is restored when it exits scope.

```csharp
// Root opens a real transaction
await using (var root = unitOfWorkManager.Begin())                          // → Root
{
    // Child delegates everything to root, CompleteAsync is a no-op here
    await using (var child = unitOfWorkManager.Begin())                     // ↳ Child of root
    {
        await child.CompleteAsync();                                        // no-op
    }

    // Sub-root RequiresNew always opens an independent transaction
    await using (var subRoot = unitOfWorkManager.Begin(
        new UnitOfWorkOptions(UnitOfWorkTransactionBehavior.RequiresNew)))  // → Root (independent)
    {
        await subRoot.CompleteAsync();                                      // commits subRoot only
    }

    await root.CompleteAsync();                                             // commits everything under root
}
```

## `IUnitOfWork`

```csharp
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Guid Id { get; }
    IUnitOfWork? Parent { get; }
    UnitOfWorkOptions Options { get; }
    Dictionary<string, object> Items { get; }
    IReadOnlyDictionary<string, UnitOfWorkDatabaseHandle> Databases { get; }
    UnitOfWorkState State { get; }

    void AddDatabase(string key, UnitOfWorkDatabaseHandle handle);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task CompleteAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
```

| Member | Description |
|---|---|
| `Id` | Unique identifier for this unit of work instance. |
| `Parent` | The outer unit of work when this one is a child or sub-root. |
| `Options` | The resolved options for this instance. |
| `Items` | A shared dictionary for passing arbitrary data within the same unit of work boundary. |
| `Databases` | All registered database handles, keyed by name. |
| `State` | Current lifecycle state. |
| `AddDatabase` | Registers a database handle to participate in save, commit, and rollback operations. |
| `SaveChangesAsync` | Flushes all pending changes across all registered database handles without committing the transaction. |
| `CompleteAsync` | Saves all changes then commits the transaction. |
| `RollbackAsync` | Rolls back the transaction without saving. |

### State

A unit of work moves through these states in order:

| State | Description |
|---|---|
| `Started` | Active and accepting operations. |
| `Committing` | `CompleteAsync` is in progress. |
| `Committed` | Transaction has been committed successfully. |
| `RollingBack` | `RollbackAsync` is in progress. |
| `RolledBack` | Transaction has been rolled back. |
| `Disposed` | The unit of work has been disposed and the ambient context restored. |

Calling `SaveChangesAsync`, `CompleteAsync`, or `RollbackAsync` on a unit of work that is not in `Started` state throws an `InvalidOperationException`. Disposing a unit of work that is already disposed is safe and does nothing.

### Child Unit of Work

A child unit of work delegates all operations to its parent. Calling `CompleteAsync` on a child is a no-op only the root's `CompleteAsync` actually commits. Calling `RollbackAsync` on a child propagates the rollback to the parent immediately.

This means you can call services that each begin their own unit of work without worrying about nested transaction conflicts. The outermost `CompleteAsync` is the one that matters.

```csharp
await using var root = unitOfWorkManager.Begin();

await orderService.PlaceOrderAsync(order);   // begins a child inside root
await inventoryService.ReserveAsync(item);   // begins another child inside root

await root.CompleteAsync();                  // commits everything
```

## Transaction Behavior

`UnitOfWorkTransactionBehavior` controls how a unit of work relates to any ambient transaction.

| Value | Description |
|---|---|
| `Required` | Joins the ambient unit of work if one exists. Creates a new transaction otherwise. |
| `RequiresNew` | Always creates a new independent transaction regardless of any ambient unit of work. |
| `Suppress` | Runs without a transaction. Each `SaveChangesAsync` call is auto-committed immediately and cannot be rolled back. |

## Interception

`Allegory.Axiom.UnitOfWork` registers `UnitOfWorkInterceptor` into the [interception](./interception.md) pipeline automatically. It opens a unit of work before each intercepted method call and completes it on success. If the method throws, the unit of work is disposed without completing, leaving the transaction uncommitted.

Intercepted methods are resolved by two mechanisms: the `IUnitOfWorkScope` marker interface and the `[UnitOfWork]` attribute.

### `IUnitOfWorkScope`

Implement `IUnitOfWorkScope` on your service interface. Every method on that service will run inside a unit of work. Methods whose names begin with a read prefix (`Get`, `Find`, `Search`, `List`, `Count`, `Exists`, `Check`, `Is`, `Has`) default to `Suppress` behavior. All other methods use `Required`.

```csharp
public interface IOrderService : IUnitOfWorkScope, ITransientService
{
    Task PlaceOrderAsync(Order order); // Required transaction
    Task<Order> GetOrderAsync(int id); // Suppress transaction (read prefix)
}

internal sealed class OrderService : IOrderService
{
    public Task PlaceOrderAsync(Order order) { ... }
    public Task<Order> GetOrderAsync(int id) { ... }
}
```

### `[UnitOfWork]` Attribute

Apply `[UnitOfWork]` at the class or method level for explicit control. A class-level attribute covers all methods unless overridden at the method level.

```csharp
[UnitOfWork]
internal sealed class OrderService : IOrderService
{
    public Task PlaceOrderAsync(Order order) { ... } // inherits from class

    [UnitOfWork(false)]
    public Task AuditOnlyAsync() { ... } // opted out

    [UnitOfWork(UnitOfWorkTransactionBehavior.RequiresNew)]
    public Task CompensateAsync() { ... } // independent transaction

    [UnitOfWork(UnitOfWorkTransactionBehavior.Required)]
    public Task GetWithLockAsync() { ... } // overrides read heuristic
}
```

The attribute exposes several constructors for common combinations:

```csharp
[UnitOfWork] // enabled, no overrides
[UnitOfWork(false)] // disabled
[UnitOfWork(UnitOfWorkTransactionBehavior.RequiresNew)] // behavior only
[UnitOfWork(IsolationLevel.Serializable)] // isolation only
[UnitOfWork(timeoutMilliseconds: 5000)] // timeout only
[UnitOfWork(UnitOfWorkTransactionBehavior.RequiresNew,
            IsolationLevel.Chaos,
            5000)] // all options
```

| Property | Type | Description |
|---|---|---|
| `IsEnabled` | `bool` | Whether the unit of work is active for this method. Defaults to `true`. |
| `TransactionBehavior` | `UnitOfWorkTransactionBehavior?` | Overrides the transaction behavior. |
| `IsolationLevel` | `IsolationLevel?` | Overrides the isolation level. |
| `Timeout` | `TimeSpan?` | Overrides the operation timeout. |

### Descriptor Resolution

For each intercepted method call, `UnitOfWorkInterceptor` resolves a descriptor that captures whether a unit of work is enabled and what options to use. The resolution order is:

1. `[UnitOfWork]` attribute on the method
2. `[UnitOfWork]` attribute on the declaring type
3. `IUnitOfWorkScope` implementation

Descriptors are cached per `MethodInfo` after the first call, so the reflection cost is paid only once.

::: info
The read-prefix heuristic always applies unless `TransactionBehavior` is explicitly specified on the `[UnitOfWork]` attribute. Methods whose names begin with `Get`, `Find`, `Search`, `List`, `Count`, `Exists`, `Check`, `Is`, or `Has` default to `Suppress`.
:::


### Customizing the Interceptor

Subclass `UnitOfWorkInterceptor` to override any part of the resolution or execution logic:

```csharp
public class MyUnitOfWorkInterceptor(IUnitOfWorkManager manager) : UnitOfWorkInterceptor(manager)
{
    protected override UnitOfWorkTransactionBehavior? TryGetDefaultBehaviour(MethodInfo methodInfo)
    {
        if (methodInfo.ReturnType == typeof(void) || methodInfo.ReturnType == typeof(Task))
            return UnitOfWorkTransactionBehavior.Suppress;

        return base.TryGetDefaultBehaviour(methodInfo);
    }
}
```