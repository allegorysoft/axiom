---
title: Exception Handling
description: Structured, localizable application exceptions with automatic HTTP error response mapping in ASP.NET Core.
---

# Exception Handling

Axiom distinguishes between expected and unexpected exceptions. An `AxiomException` is an expected exception one you intentionally throw to signal a known failure condition such as a missing resource, a failed authorization check, or a violated business rule. Unlike unhandled exceptions, they carry a code, an optional message, and arbitrary data entries that the framework uses to produce a structured response.

The base class for all Axiom exceptions is `AxiomException`. Several subtypes are provided out of the box:

| Exception | Intended use |
|---|---|
| `AuthorizationException` | The caller lacks permission |
| `BusinessException` | A violated business rule or precondition |
| `NotFoundException` | A requested resource does not exist |

Throw them from anywhere in your application logic:

```csharp
throw new NotFoundException(
     code: "ORDER:404", message: "Order {id} not found"
    ).AddData("id", orderId);
```

## ASP.NET Core Integration

In ASP.NET Core applications, Axiom maps `AxiomException` instances to structured HTTP problem details responses. This is implemented as an `IExceptionHandler` that plugs into the standard ASP.NET Core exception handling middleware.

```bash
dotnet add package Allegory.Axiom.AspNetCore.ExceptionHandling
```

### Setup

Add the ASP.NET Core exception handling middleware to your pipeline:

```csharp
// In your middleware pipeline
app.UseExceptionHandler();
```

### How It Works

When an exception propagates out of your middleware pipeline, ASP.NET Core's exception handler middleware invokes registered `IExceptionHandler` implementations in order. `AxiomExceptionHandler` checks whether the exception is an `AxiomException`. If it is not, it returns `false` and the exception is passed to the next handler. If it is, it:

1. Logs if the configured log level exists for that exception type
2. Resolves a HTTP status code from the exception type using the configured mappings
3. Sets `HttpContext.Response.StatusCode` to that code
4. Builds a `ProblemDetails` response with `title` set to the exception code and `detail` set to the message
5. Optionally localizes the detail if no message is set and the exception code prefix has a registered resource mapping
6. Interpolates any `Exception.Data` entries into the message using `{key}` placeholders and adds them as problem details extensions

### Default Mappings

These are registered as defaults:

| Exception | Status Code | Log Level |
|---|---|---|
| `AuthorizationException` | 403 Forbidden | Warning |
| `NotFoundException` | 404 Not Found | - |
| `BusinessException` | 409 Conflict | - |

If an exception type is not directly mapped, Axiom walks up the class hierarchy until a mapped base type is found. If no mapping exists at any level, the response status code is left unchanged and nothing is logged.

```csharp
// Inherits NotFoundException mapping → 404 Not Found
public class OrderNotFoundException(string? code = null, string? message = null)
    : NotFoundException(code, message);
```

### Exception Data

Entries in `Exception.Data` are interpolated into the message using `{key}` placeholders and also added as extensions on the problem details response. The placeholder name must match the data key exactly.

```csharp
var ex = new BusinessException(
     code: "BIZ:001", message: "Entity {id} not found"
    ).AddData("id", 42);

// ProblemDetails:
// title:   "BIZ:001"
// detail:  "Entity 42 not found"
// id:      42
```

### Localization

If an exception has no message but has a code, the handler can look up the detail from a string localizer. The code must contain a `:` separator and the prefix before it must be [mapped](./localization#exception-code-mapping) to a resource name via `MapExceptionCode` in `LocalizationOptions`.

```csharp
internal sealed class MyAppPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<LocalizationOptions>(options =>
        {
            // Map prefix "BIZ" to the MyApp.Resources.Messages resource
            options.MapExceptionCode("BIZ", "MyApp.Resources.Messages");

            // Or map by resource marker type
            options.MapExceptionCode<MyAppResource>("BIZ");
        });
        return Task.CompletedTask;
    }
}
```

With this configuration, a `BusinessException` with code `BIZ:001` and no message resolves its detail by calling `IStringLocalizer["BIZ:001"]` on the `MyApp.Resources.Messages` resource.

Localization is skipped when:

- The exception already has a non-empty message
- The code contains no `:` separator
- The code prefix has no entry in `ExceptionCodeMappings`

```csharp
// Localized no message, prefix "BIZ" is mapped
throw new BusinessException(code: "BIZ:001");

// Not localized message present, localizer never called
throw new BusinessException(code: "BIZ:001", message: "Order already placed");

// Not localized no colon separator in code
throw new BusinessException(code: "NoPrefixCode");
```

See [Localization](./localization) for configuring translation files and file providers.

### Problem Details Shape

Every handled exception produces a response conforming to [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457). The `title` is the exception code, `detail` is the resolved message, and exception data entries appear as top-level extensions:

```json
{
  "title": "BIZ:001",
  "detail": "Entity 42 not found",
  "status": 409,
  "id": 42
}
```

### Custom Exception Types

Define your own exception types by subclassing `AxiomException` or any of its subtypes:

```csharp
public class OrderCalculationException() : AxiomException("ORD:CalculationFailed", "Failed to calculate order total");
```

To map it to a status code and log level, configure `AspNetCoreExceptionHandlerOptions` in your application package:

```csharp
internal sealed class MyAppPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<AspNetCoreExceptionHandlerOptions>(options =>
        {
            options.AddStatusCode<OrderCalculationException>(HttpStatusCode.BadRequest);
            options.AddLogLevel<OrderCalculationException>(LogLevel.Error);
        });

        return Task.CompletedTask;
    }
}
```