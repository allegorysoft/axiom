using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Exceptions;
using Allegory.Axiom.Security;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.MultiTenancy;

public class CurrentTenantCheckerTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected ICurrentTenantChecker Checker { get; } = fixture.Service<ICurrentTenantChecker>();
    protected ITenantStore TenantStore { get; } = fixture.Service<ITenantStore>();

    [Fact]
    public async Task ShouldPassWhenPrincipalIsNull()
    {
        var tenant = await TenantStore.FindAsync("t-1");
        await Should.NotThrowAsync(() => Checker.CheckAsync(tenant!));
    }

    [Fact]
    public async Task ShouldPassWhenIdentityIsNotAuthenticated()
    {
        var tenant = await TenantStore.FindAsync("t-1");
        Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        await Should.NotThrowAsync(() => Checker.CheckAsync(tenant!));
    }

    [Fact]
    public async Task ShouldThrowWhenAuthenticatedPrincipalHasNoNameIdentifierClaim()
    {
        var tenant = await TenantStore.FindAsync("t-1");
        Thread.CurrentPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity([], authenticationType: "test"));// no NameIdentifier

        var ex = await Should.ThrowAsync<AuthorizationException>(() =>
            Checker.CheckAsync(tenant!));

        ex.Code.ShouldBe(SecurityExceptionCodes.NameIdentifierNotFound);
    }
    
    [Fact]
    public async Task ShouldThrowWhenPrincipalDoesNotHaveAccessToTenant()
    {
        var tenant = await TenantStore.FindAsync("t-1");
        Thread.CurrentPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "123")], authenticationType: "test"));

        var ex = await Should.ThrowAsync<AuthorizationException>(() =>
            Checker.CheckAsync(tenant!));

        ex.Data["tenantId"].ShouldBe(tenant!.Id);
        ex.Data["principalId"].ShouldBe(Thread.CurrentPrincipal.Identity!.GetNameIdentifier());
    }
    
    [Fact]
    public async Task ShouldPassWhenPrincipalHasAccessToTenant()
    {
        var tenant = await TenantStore.FindAsync("t-1");
        Thread.CurrentPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000000")],
                authenticationType: "test"));


        await Should.NotThrowAsync(() => Checker.CheckAsync(tenant!));
    }
}