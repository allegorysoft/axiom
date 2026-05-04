using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Security.Principal;

public class HttpContextPrincipalAccessorTests
{
    public HttpContextPrincipalAccessorTests()
    {
        HttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        Accessor = new HttpContextPrincipalAccessor(HttpContextAccessor);
    }

    protected IHttpContextAccessor HttpContextAccessor { get; }
    protected HttpContextPrincipalAccessor Accessor { get; }

    [Fact]
    public void ShouldReturnNullWhenHttpContextIsNull()
    {
        HttpContextAccessor.HttpContext.Returns((HttpContext?) null);

        Accessor.Current.ShouldBeNull();
    }

    [Fact]
    public void ShouldReturnUserFromHttpContext()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "test")], "mock"));
        HttpContextAccessor.HttpContext.Returns(new DefaultHttpContext {User = principal});

        Accessor.Current.ShouldBe(principal);
    }

    [Fact]
    public void ShouldReflectHttpContextUserChanges()
    {
        var first = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "first")], "mock"));
        var second = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "second")], "mock"));

        HttpContextAccessor.HttpContext.Returns(new DefaultHttpContext {User = first});
        Accessor.Current.ShouldBe(first);

        HttpContextAccessor.HttpContext.Returns(new DefaultHttpContext {User = second});
        Accessor.Current.ShouldBe(second);
    }

    [Fact]
    public void ShouldFallbackToBaseWhenHttpContextIsNull()
    {
        HttpContextAccessor.HttpContext.Returns((HttpContext?) null);
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "test")], "mock"));
        Thread.CurrentPrincipal = principal;

        Accessor.Current.ShouldBe(principal);
    }
}