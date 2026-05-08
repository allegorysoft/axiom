using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.MultiTenancy;

public class HttpContextCurrentTenantIdentifierProviderTests
{
    public HttpContextCurrentTenantIdentifierProviderTests()
    {
        HttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        Options = Microsoft.Extensions.Options.Options.Create(new AspNetCoreMultiTenancyOptions());
        Provider = new HttpContextCurrentTenantIdentifierProvider(HttpContextAccessor, Options);
    }

    protected IHttpContextAccessor HttpContextAccessor { get; }
    protected IOptions<AspNetCoreMultiTenancyOptions> Options { get; }
    protected HttpContextCurrentTenantIdentifierProvider Provider { get; }

    private static DefaultHttpContext CreateContext(
        string? headerValue = null,
        string? queryValue = null,
        string? routeValue = null,
        string headerKey = "X-Tenant",
        string queryKey = "__tenant",
        string routeKey = "tenant")
    {
        var context = new DefaultHttpContext();
        if (headerValue != null) context.Request.Headers[headerKey] = headerValue;
        if (queryValue != null) context.Request.QueryString = new QueryString($"?{queryKey}={queryValue}");
        if (routeValue != null) context.Request.RouteValues[routeKey] = routeValue;
        return context;
    }

    [Fact]
    public async Task ShouldReturnNullWhenHttpContextIsNull()
    {
        HttpContextAccessor.HttpContext.Returns((HttpContext?) null);

        var result = await Provider.TryGetAsync();

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldReturnNullWhenNoIdentifierPresent()
    {
        HttpContextAccessor.HttpContext.Returns(new DefaultHttpContext());

        var result = await Provider.TryGetAsync();

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldReturnTenantFromHeader()
    {
        HttpContextAccessor.HttpContext.Returns(CreateContext(headerValue: "tenant-a"));

        var result = await Provider.TryGetAsync();

        result.ShouldBe("tenant-a");
    }

    [Fact]
    public async Task ShouldReturnTenantFromQuery()
    {
        HttpContextAccessor.HttpContext.Returns(CreateContext(queryValue: "tenant-b"));

        var result = await Provider.TryGetAsync();

        result.ShouldBe("tenant-b");
    }

    [Fact]
    public async Task ShouldReturnTenantFromRoute()
    {
        HttpContextAccessor.HttpContext.Returns(CreateContext(routeValue: "tenant-c"));

        var result = await Provider.TryGetAsync();

        result.ShouldBe("tenant-c");
    }

    [Fact]
    public async Task ShouldPreferHeaderOverQuery()
    {
        HttpContextAccessor.HttpContext.Returns(CreateContext(headerValue: "from-header", queryValue: "from-query"));

        var result = await Provider.TryGetAsync();

        result.ShouldBe("from-header");
    }

    [Fact]
    public async Task ShouldPreferQueryOverRoute()
    {
        HttpContextAccessor.HttpContext.Returns(CreateContext(queryValue: "from-query", routeValue: "from-route"));

        var result = await Provider.TryGetAsync();

        result.ShouldBe("from-query");
    }

    [Fact]
    public async Task ShouldRespectCustomHeaderKey()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new AspNetCoreMultiTenancyOptions {HeaderKey = "X-Custom"});
        var provider = new HttpContextCurrentTenantIdentifierProvider(HttpContextAccessor, options);
        HttpContextAccessor.HttpContext.Returns(CreateContext(headerValue: "custom-tenant", headerKey: "X-Custom"));

        var result = await provider.TryGetAsync();

        result.ShouldBe("custom-tenant");
    }

    [Fact]
    public async Task ShouldRespectCustomQueryKey()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new AspNetCoreMultiTenancyOptions {QueryKey = "tid"});
        var provider = new HttpContextCurrentTenantIdentifierProvider(HttpContextAccessor, options);
        HttpContextAccessor.HttpContext.Returns(CreateContext(queryValue: "custom-tenant", queryKey: "tid"));

        var result = await provider.TryGetAsync();

        result.ShouldBe("custom-tenant");
    }

    [Fact]
    public async Task ShouldRespectCustomRouteKey()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new AspNetCoreMultiTenancyOptions {RouteKey = "t"});
        var provider = new HttpContextCurrentTenantIdentifierProvider(HttpContextAccessor, options);
        HttpContextAccessor.HttpContext.Returns(CreateContext(routeValue: "custom-tenant", routeKey: "t"));

        var result = await provider.TryGetAsync();

        result.ShouldBe("custom-tenant");
    }
}