using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Security.Principal;

public class ClaimsPrincipalAccessorTests : HostedIntegrationTestBase
{
    protected IPrincipalAccessor Accessor => Service<IPrincipalAccessor>();

    [Fact]
    public void ShouldReturnCurrentClaimsPrincipal()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "test")], "mock"));

        Accessor.Current.ShouldBeNull();

        Thread.CurrentPrincipal = principal;

        Accessor.Current.ShouldBe(principal);
    }

    [Fact]
    public void ShouldReflectCurrentPrincipalChanges()
    {
        var first = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "first")], "mock"));
        var second = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "second")], "mock"));

        Thread.CurrentPrincipal = first;
        Accessor.Current.ShouldBe(first);

        Thread.CurrentPrincipal = second;
        Accessor.Current.ShouldBe(second);
    }

    [Fact]
    public async Task ShouldNotLeakPrincipalChangedInsideAsyncContextToOuterContext()
    {
        var outer = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "outer")], "mock"));
        Thread.CurrentPrincipal = outer;
        Accessor.Current.ShouldBe(outer);

        await Task.Run(async () =>
            {
                await Task.Yield();
                Accessor.Current.ShouldBe(outer);
                var inner = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "inner")], "mock"));
                Thread.CurrentPrincipal = inner;
                Accessor.Current.ShouldBe(inner);
            }
            , TestContext.Current.CancellationToken);

        Accessor.Current.ShouldBe(outer);
    }
}