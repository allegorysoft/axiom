using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.MultiTenancy;

public class HttpContextCurrentTenantIdentifierProvider(
    IHttpContextAccessor httpContextAccessor,
    IOptions<AspNetCoreMultiTenancyOptions> options)
    : ICurrentTenantIdentifierProvider
{
    public AspNetCoreMultiTenancyOptions Options { get; } = options.Value;

    public virtual ValueTask<string?> TryGetAsync()
    {
        var context = httpContextAccessor.HttpContext;
        return context == null ? ValueTask.FromResult<string?>(null) : TryGetAsync(context);
    }

    protected virtual ValueTask<string?> TryGetAsync(HttpContext context)
    {
        var identifier = TryGetFromHeader(context)
                     ?? TryGetFromQuery(context)
                     ?? TryGetFromRoute(context);

        return ValueTask.FromResult(identifier);
    }

    protected virtual string? TryGetFromHeader(HttpContext context)
    {
        return context.Request.Headers.TryGetValue(Options.HeaderKey, out var val) ? (string?) val : null;
    }

    protected virtual string? TryGetFromQuery(HttpContext context)
    {
        return context.Request.Query.TryGetValue(Options.QueryKey, out var identifier)
            ? (string?) identifier
            : null;
    }

    protected virtual string? TryGetFromRoute(HttpContext context)
    {
        return context.GetRouteValue(Options.RouteKey)?.ToString();
    }
}