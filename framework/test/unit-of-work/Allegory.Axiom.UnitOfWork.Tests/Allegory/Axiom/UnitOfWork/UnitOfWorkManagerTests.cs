using Microsoft.Testing.Platform.Services;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkManagerTests : IntegrationTestBase
{
    [Fact]
    public void ShouldCreateUnitOfWork()
    {
        var manager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        using (var root = manager.Begin())
        {
            manager.Current.ShouldNotBeNull();
            manager.Current.ShouldBe(root);
        }

        manager.Current.ShouldBeNull();
    }

    [Fact]
    public void ShouldJoinParentUnitOfWorkWhenParentExists()
    {
        var manager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        using (var root = manager.Begin())
        {
            manager.Current.ShouldBe(root);
            manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var child = manager.Begin())
            {
                manager.Current.ShouldBe(child);
                manager.Current.ShouldBeOfType<ChildUnitOfWork>();
                manager.Current.Parent.ShouldBe(root);
            }
        }
    }

    [Fact]
    public void ShouldRestoreParentAfterChildDisposed()
    {
        var manager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        using (var root = manager.Begin())
        {
            manager.Current.ShouldBe(root);

            using (var child = manager.Begin())
            {
                manager.Current.ShouldBe(child);
            }

            manager.Current.ShouldBe(root);
        }
    }
}