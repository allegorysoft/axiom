using System;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkManagerTests : IntegrationTestBase
{
    public UnitOfWorkManagerTests()
    {
        Builder.Services.Configure<UnitOfWorkOptions>(options =>
        {
            options.Timeout = TimeSpan.FromSeconds(30);
        });
    }

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
    public void ShouldCreateChildUnitOfWorkWhenParentExists()
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
    public void ShouldRestoreParentUnitOfWorkAfterChildUnitOfWorkDisposed()
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

    [Fact]
    public void ShouldUseParentPropertiesWhenUnitOfWorkIsChild()
    {
        var manager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        using (var root = manager.Begin())
        {
            root.Items["key"] = "value";
            using (var child = manager.Begin())
            {
                manager.Current!.Items["key"].ShouldBe("value");
                root.Items.ShouldBe(child.Items);
            }
        }
    }

    [Fact]
    public void ShouldCreateSubRootUnitOfWorkWhenTransactionBehaviorIsRequiresNew()
    {
        var manager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        using (var root = manager.Begin())
        {
            manager.Current.ShouldBe(root);
            manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var subRoot = manager.Begin(new UnitOfWorkOptions(
                       transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew)))
            {
                manager.Current.ShouldBe(subRoot);
                manager.Current.ShouldBeOfType<UnitOfWork>();
            }

            manager.Current.ShouldBe(root);
        }
    }

    [Fact]
    public void ShouldCreateChildUnitOfWorkWhenTransactionBehaviorCompatible()
    {
        var manager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        // Required, Required
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

        // RequiresNew, Required
        using (var root = manager.Begin(new UnitOfWorkOptions(
                   transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew)))
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

        // Suppress, Suppress
        using (var root = manager.Begin(new UnitOfWorkOptions(
                   transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
        {
            manager.Current.ShouldBe(root);
            manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var child = manager.Begin(new UnitOfWorkOptions(
                       transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
            {
                manager.Current.ShouldBe(child);
                manager.Current.ShouldBeOfType<ChildUnitOfWork>();
                manager.Current.Parent.ShouldBe(root);
            }
        }
    }

    [Fact]
    public void ShouldCreateSubRootUnitOfWorkWhenTransactionBehaviorIncompatible()
    {
        var manager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        // Required, Suppress
        using (var root = manager.Begin())
        {
            manager.Current.ShouldBe(root);
            manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var subRoot = manager.Begin(new UnitOfWorkOptions(
                       transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
            {
                manager.Current.ShouldBe(subRoot);
                manager.Current.ShouldBeOfType<UnitOfWork>();
                manager.Current.Parent.ShouldBe(root);
            }
        }

        // Suppress, Required
        using (var root = manager.Begin(new UnitOfWorkOptions(
                   transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
        {
            manager.Current.ShouldBe(root);
            manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var subRoot = manager.Begin())
            {
                manager.Current.ShouldBe(subRoot);
                manager.Current.ShouldBeOfType<UnitOfWork>();
                manager.Current.Parent.ShouldBe(root);
            }
        }

        // RequiresNew, Suppress
        using (var root = manager.Begin(new UnitOfWorkOptions(
                   transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew)))
        {
            manager.Current.ShouldBe(root);
            manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var subRoot = manager.Begin(new UnitOfWorkOptions(
                       transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
            {
                manager.Current.ShouldBe(subRoot);
                manager.Current.ShouldBeOfType<UnitOfWork>();
                manager.Current.Parent.ShouldBe(root);
            }
        }
    }

    [Fact]
    public void ShouldApplyDefaultOptionsWhenPreferredOptionsNull()
    {
        var manager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var options = ServiceProvider.GetRequiredService<IOptions<UnitOfWorkOptions>>().Value;

        using var uow = manager.Begin();

        manager.Current!.Options.ShouldBe(options);
        manager.Current!.Options.Timeout.ShouldBe(options.Timeout);
    }

    [Fact]
    public void ShouldApplyPreferredOptionsWhenPreferredOptionsNotNull()
    {
        var manager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        var preferred = new UnitOfWorkOptions(timeout: TimeSpan.FromMinutes(1));
        using var uow = manager.Begin(preferred);

        manager.Current!.Options.ShouldBe(preferred);
        manager.Current!.Options.Timeout.ShouldBe(preferred.Timeout);
    }

    [Fact]
    public void ShouldFallbackDefaultOptionsWhenPreferredOptionsPropertyIsNull()
    {
        var manager = ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var options = ServiceProvider.GetRequiredService<IOptions<UnitOfWorkOptions>>().Value;

        var preferred = new UnitOfWorkOptions(isolationLevel: IsolationLevel.ReadUncommitted);
        using var uow = manager.Begin(preferred);

        manager.Current!.Options.ShouldBe(preferred);
        manager.Current!.Options.IsolationLevel.ShouldBe(preferred.IsolationLevel);
        manager.Current!.Options.Timeout.ShouldBe(options.Timeout);
    }
}