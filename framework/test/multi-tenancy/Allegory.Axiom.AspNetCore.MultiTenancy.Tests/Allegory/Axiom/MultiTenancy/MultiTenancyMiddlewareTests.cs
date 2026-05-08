using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.MultiTenancy;

public class MultiTenancyMiddlewareTests
{
    public MultiTenancyMiddlewareTests()
    {
        CurrentTenantProvider = Substitute.For<ICurrentTenantProvider>();
        TenantContextAccessor = Substitute.For<ITenantContextAccessor>();
        Middleware = new MultiTenancyMiddleware(
            _ => Task.CompletedTask,
            CurrentTenantProvider,
            TenantContextAccessor);
    }

    protected ICurrentTenantProvider CurrentTenantProvider { get; }
    protected ITenantContextAccessor TenantContextAccessor { get; }
    protected MultiTenancyMiddleware Middleware { get; }

    private static TenantContext CreateTenant(string name = "TestTenant") =>
        new(Guid.NewGuid(), name, name.ToUpperInvariant());

    [Fact]
    public async Task ShouldSetTenantWhenTenantFound()
    {
        var tenant = CreateTenant();
        CurrentTenantProvider.TryGetAsync().Returns(tenant);
        var context = new DefaultHttpContext();

        await Middleware.InvokeAsync(context);

        TenantContextAccessor.Received(1).Set(tenant);
    }

    [Fact]
    public async Task ShouldNotSetTenantWhenTenantNotFound()
    {
        CurrentTenantProvider.TryGetAsync().Returns((TenantContext?) null);
        var context = new DefaultHttpContext();

        await Middleware.InvokeAsync(context);

        TenantContextAccessor.DidNotReceive().Set(Arg.Any<TenantContext>());
    }

    [Fact]
    public async Task ShouldAlwaysCallNext()
    {
        CurrentTenantProvider.TryGetAsync().Returns((TenantContext?) null);
        var isNextCalled = false;
        var middleware = new MultiTenancyMiddleware(
            _ =>
            {
                isNextCalled = true;
                return Task.CompletedTask;
            },
            CurrentTenantProvider,
            TenantContextAccessor);

        await middleware.InvokeAsync(new DefaultHttpContext());

        isNextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldSetTenantBeforeCallingNextWhenTenantIsFound()
    {
        var callOrder = new List<string>();
        TenantContextAccessor.When(x => x.Set(Arg.Any<TenantContext>()))
            .Do(_ => callOrder.Add("set"));
        var middleware = new MultiTenancyMiddleware(
            _ =>
            {
                callOrder.Add("next");
                return Task.CompletedTask;
            },
            CurrentTenantProvider,
            TenantContextAccessor);
        CurrentTenantProvider.TryGetAsync().Returns(CreateTenant());

        await middleware.InvokeAsync(new DefaultHttpContext());

        callOrder.ShouldBe(["set", "next"]);
    }
}