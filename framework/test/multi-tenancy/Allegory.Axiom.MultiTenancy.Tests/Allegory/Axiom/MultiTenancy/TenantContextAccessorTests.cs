using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.MultiTenancy;

public class TenantContextAccessorTests
{
    protected TenantContextAccessor Accessor { get; } = new();

    private static TenantContext CreateTenant(string name = "TestTenant") =>
        new(Guid.NewGuid(), name, name.ToUpperInvariant());

    [Fact]
    public void ShouldReturnNullWhenNoContextSet()
    {
        Accessor.Current.ShouldBeNull();
    }

    [Fact]
    public void ShouldReturnContextAfterSet()
    {
        var tenant = CreateTenant();

        Accessor.Set(tenant);

        Accessor.Current.ShouldBe(tenant);
    }

    [Fact]
    public void ShouldReturnNullAfterSetWithNull()
    {
        Accessor.Set(CreateTenant());
        Accessor.Current.ShouldNotBeNull();

        Accessor.Set(current: null);
        Accessor.Current.ShouldBeNull();
    }

    [Fact]
    public void ShouldRestorePreviousContextAfterChangeDisposed()
    {
        var original = CreateTenant("Original");
        var temporary = CreateTenant("Temporary");

        Accessor.Set(original);

        using (Accessor.Change(temporary))
        {
            Accessor.Current.ShouldBe(temporary);
        }

        Accessor.Current.ShouldBe(original);
    }

    [Fact]
    public void ShouldRestoreNullAfterChangeDisposedWhenNoOriginalContext()
    {
        Accessor.Current.ShouldBeNull();

        using (Accessor.Change(CreateTenant()))
        {
            Accessor.Current.ShouldNotBeNull();
        }

        Accessor.Current.ShouldBeNull();
    }

    [Fact]
    public void ShouldSetNullContextWithChange()
    {
        Accessor.Set(CreateTenant());
        Accessor.Current.ShouldNotBeNull();

        using (Accessor.Change(current: null))
        {
            Accessor.Current.ShouldBeNull();
        }
    }

    [Fact]
    public async Task ShouldIsolateContextPerAsyncFlow()
    {
        var tenant1 = CreateTenant("Tenant1");
        var tenant2 = CreateTenant("Tenant2");

        Accessor.Set(tenant1);

        await Task.Run(() =>
        {
            // AsyncLocal: child flow inherits value at fork point but changes don't propagate back
            Accessor.Current.ShouldBe(tenant1);

            Accessor.Set(tenant2);
            Accessor.Current.ShouldBe(tenant2);

        }, TestContext.Current.CancellationToken);

        Accessor.Current.ShouldBe(tenant1);// parent flow unaffected
    }

    [Fact]
    public async Task ShouldScopeChangeToCurrentAsyncContext()
    {
        var outer = CreateTenant("Outer");
        var inner = CreateTenant("Inner");

        Accessor.Set(outer);

        await Task.Run(async () =>
        {
            Accessor.Current.ShouldBe(outer);

            using (Accessor.Change(inner))
            {
                Accessor.Current.ShouldBe(inner);
                await Task.Yield();
                Accessor.Current.ShouldBe(inner);
            }

            Accessor.Current.ShouldBe(outer);
        }, TestContext.Current.CancellationToken);
    }

    [Fact]
    public void ShouldSupportNestedChanges()
    {
        var first = CreateTenant("First");
        var second = CreateTenant("Second");
        var third = CreateTenant("Third");

        Accessor.Set(first);

        using (Accessor.Change(second))
        {
            Accessor.Current.ShouldBe(second);

            using (Accessor.Change(third))
            {
                Accessor.Current.ShouldBe(third);
            }

            Accessor.Current.ShouldBe(second);
        }

        Accessor.Current.ShouldBe(first);
    }

    [Fact]
    public void ShouldExposeCorrectIdAndNameOnContext()
    {
        var id = Guid.NewGuid();
        var tenant = new TenantContext(id, "my-tenant", "MY-TENANT");

        Accessor.Set(tenant);

        Accessor.Current!.Id.ShouldBe(id);
        Accessor.Current.Name.ShouldBe("my-tenant");
        Accessor.Current.NormalizedName.ShouldBe("MY-TENANT");
    }
}