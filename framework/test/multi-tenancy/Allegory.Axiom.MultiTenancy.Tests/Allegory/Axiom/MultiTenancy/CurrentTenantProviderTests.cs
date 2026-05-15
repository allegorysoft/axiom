using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.MultiTenancy;

public class CurrentTenantProviderTests : IntegrationTest
{
    public override ValueTask InitializeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ShouldReturnNullWhenNoProviderReturnsIdentifier()
    {
        var p1 = Substitute.For<ICurrentTenantIdentifierProvider>();
        var p2 = Substitute.For<ICurrentTenantIdentifierProvider>();
        p1.TryGetAsync().Returns((string?) null);
        p2.TryGetAsync().Returns(string.Empty);

        var provider = new CurrentTenantProvider(
            [p1, p2],
            Substitute.For<ITenantStore>(),
            Substitute.For<ICurrentTenantChecker>());

        var result = await provider.TryGetAsync();

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldReturnNullWhenIdentifierProviderListIsEmpty()
    {
        var provider = new CurrentTenantProvider(
            [],
            Substitute.For<ITenantStore>(),
            Substitute.For<ICurrentTenantChecker>());

        var result = await provider.TryGetAsync();
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldResolveByIdWhenIdentifierIsGuid()
    {
        var services = await CreateServiceProviderAsync(builder =>
        {
            var provider = Substitute.For<ICurrentTenantIdentifierProvider>();
            provider.TryGetAsync().Returns("00000000-0000-0000-0000-000000000001");
            builder.Services.AddSingleton(provider);
        });

        var provider = services.GetRequiredService<ICurrentTenantProvider>();
        var result = await provider.TryGetAsync();

        result.ShouldNotBeNull();
        result.Id.ShouldBe(new Guid("00000000-0000-0000-0000-000000000001"));
        result.NormalizedName.ShouldBe("T-2");
    }

    [Fact]
    public async Task ShouldResolveByNameWhenIdentifierIsName()
    {
        var services = await CreateServiceProviderAsync(builder =>
        {
            var provider = Substitute.For<ICurrentTenantIdentifierProvider>();
            provider.TryGetAsync().Returns("t-1");
            builder.Services.AddSingleton(provider);
        });

        var provider = services.GetRequiredService<ICurrentTenantProvider>();
        var result = await provider.TryGetAsync();

        result.ShouldNotBeNull();
        result.Id.ShouldBe(new Guid("00000000-0000-0000-0000-000000000000"));
        result.NormalizedName.ShouldBe("T-1");
    }

    [Fact]
    public async Task ShouldStopAtFirstProviderThatReturnsIdentifier()
    {
        var p1 = Substitute.For<ICurrentTenantIdentifierProvider>();
        var p2 = Substitute.For<ICurrentTenantIdentifierProvider>();
        var p3 = Substitute.For<ICurrentTenantIdentifierProvider>();
        p1.TryGetAsync().Returns((string?) null);
        p2.TryGetAsync().Returns("t-1");
        p2.TryGetAsync().Returns("t-2");

        var services = await CreateServiceProviderAsync(builder =>
        {
            builder.Services.AddSingleton(p1);
            builder.Services.AddSingleton(p2);
            builder.Services.AddSingleton(p3);
        });

        var provider = services.GetRequiredService<ICurrentTenantProvider>();
        await provider.TryGetAsync();

        await p1.Received(1).TryGetAsync();
        await p2.Received(1).TryGetAsync();
        await p3.Received(0).TryGetAsync();
    }

    [Fact]
    public async Task ShouldThrowWhenTenantNotFoundById()
    {
        var missingId = Guid.NewGuid();
        var services = await CreateServiceProviderAsync(builder =>
        {
            var provider = Substitute.For<ICurrentTenantIdentifierProvider>();
            provider.TryGetAsync().Returns(missingId.ToString());
            builder.Services.AddSingleton(provider);
        });

        var provider = services.GetRequiredService<ICurrentTenantProvider>();
        var result = await Should.ThrowAsync<NotFoundException>(async () => await provider.TryGetAsync());

        result.Code.ShouldBe(MultiTenancyExceptionCodes.TenantNotFound);
        result.Data["identifier"].ShouldBe(missingId.ToString());
    }

    [Fact]
    public async Task ShouldThrowWhenTenantNotFoundByName()
    {
        var services = await CreateServiceProviderAsync(builder =>
        {
            var provider = Substitute.For<ICurrentTenantIdentifierProvider>();
            provider.TryGetAsync().Returns("ghost");
            builder.Services.AddSingleton(provider);
        });

        var provider = services.GetRequiredService<ICurrentTenantProvider>();
        var result = await Should.ThrowAsync<NotFoundException>(async () => await provider.TryGetAsync());

        result.Code.ShouldBe(MultiTenancyExceptionCodes.TenantNotFound);
        result.Data["identifier"].ShouldBe("ghost");
    }

    //ShouldThrowWhenTenantIsNotActive

    [Fact]
    public async Task ShouldResolveWhenPrincipalHasAccessToTenant()
    {
        var services = await CreateServiceProviderAsync(builder =>
        {
            var provider = Substitute.For<ICurrentTenantIdentifierProvider>();
            provider.TryGetAsync().Returns("00000000-0000-0000-0000-000000000000");
            builder.Services.AddSingleton(provider);
        });

        Thread.CurrentPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000000")],
                authenticationType: "test"));

        var provider = services.GetRequiredService<ICurrentTenantProvider>();
        var tenant = await provider.TryGetAsync();

        tenant.ShouldNotBeNull();
        tenant.NormalizedName.ShouldBe("T-1");
    }

    [Fact]
    public async Task ShouldThrowWhenPrincipalDoesNotHaveAccessToTenant()
    {
        var services = await CreateServiceProviderAsync(builder =>
        {
            var provider = Substitute.For<ICurrentTenantIdentifierProvider>();
            provider.TryGetAsync().Returns("00000000-0000-0000-0000-000000000000");
            builder.Services.AddSingleton(provider);
        });

        Thread.CurrentPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000001")],
                authenticationType: "test"));

        var provider = services.GetRequiredService<ICurrentTenantProvider>();
        await Should.ThrowAsync<Exception>(async () => await provider.TryGetAsync());
    }
}