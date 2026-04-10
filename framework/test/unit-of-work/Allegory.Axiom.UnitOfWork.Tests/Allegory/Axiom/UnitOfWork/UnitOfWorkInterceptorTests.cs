using System;
using System.Data;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkInterceptorTests : IntegrationTestBase
{
    protected IUnitOfWorkManager UnitOfWorkManager => Service<IUnitOfWorkManager>();

    [Fact]
    public async Task ShouldBeginUnitOfWorkWhenServiceHasMarkerInterface()
    {
        var service = Service<IUnitOfWorkScopedService>();

        service.OnExecute = () =>
        {
            UnitOfWorkManager.Current.ShouldNotBeNull();
            UnitOfWorkManager.Current.Options.TransactionBehavior.ShouldBe(UnitOfWorkTransactionBehavior.Required);
            UnitOfWorkManager.Current.State.ShouldBe(UnitOfWorkState.Started);
        };

        await service.DoWorkAsync();
    }

    [Fact]
    public async Task ShouldBeginUnitOfWorkWhenServiceHasAttribute()
    {
        var service = Service<IAttributedUnitOfWorkService>();

        service.OnExecute = () =>
        {
            UnitOfWorkManager.Current.ShouldNotBeNull();
            UnitOfWorkManager.Current.Options.TransactionBehavior.ShouldBe(UnitOfWorkTransactionBehavior.Required);
            UnitOfWorkManager.Current.State.ShouldBe(UnitOfWorkState.Started);
        };

        await service.DoWorkAsync();
    }

    [Fact]
    public async Task ShouldNotBeginUnitOfWorkWhenAttributeDisables()
    {
        var service = Service<IAttributedUnitOfWorkService>();

        service.OnExecute = () =>
        {
            UnitOfWorkManager.Current.ShouldBeNull();
        };

        await service.SkippedAsync();
    }

    [Fact]
    public async Task ShouldApplyAttributeOptions()
    {
        var service = Service<IAttributedUnitOfWorkService>();

        service.OnExecute = () =>
        {
            var options = UnitOfWorkManager.Current!.Options;
            options.TransactionBehavior.ShouldBe(UnitOfWorkTransactionBehavior.RequiresNew);
            options.IsolationLevel.ShouldBe(IsolationLevel.Chaos);
            options.Timeout.ShouldBe(TimeSpan.FromMilliseconds(5000));
        };

        await service.DoWorkOptionedAsync();
    }

    [Fact]
    public async Task ShouldSuppressTransactionForReadPrefixedMethods()
    {
        var service = Service<IAttributedUnitOfWorkService>();

        service.OnExecute = () =>
        {
            UnitOfWorkManager.Current!.Options.TransactionBehavior.ShouldBe(UnitOfWorkTransactionBehavior.Suppress);
        };

        await service.GetAsync();
    }

    [Fact]
    public async Task ShouldOverrideReadHeuristicWhenMethodAttributeSpecifiesBehavior()
    {
        var service = Service<IAttributedUnitOfWorkService>();

        service.OnExecute = () =>
        {
            UnitOfWorkManager.Current?.Options.TransactionBehavior.ShouldBe(UnitOfWorkTransactionBehavior.Required);
        };

        await service.GetOptionedAsync();
    }

    [Fact]
    public async Task ShouldCacheDescriptorForSameMethod()
    {
        var service = Service<IAttributedUnitOfWorkService>();
        var interceptor = Service<UnitOfWorkInterceptor>();

        interceptor.MethodInfoCache.Count.ShouldBe(0);

        await service.DoWorkAsync();
        var cacheCountAfterFirst = interceptor.MethodInfoCache.Count;

        await service.DoWorkAsync();
        var cacheCountAfterSecond = interceptor.MethodInfoCache.Count;

        cacheCountAfterFirst.ShouldBe(cacheCountAfterSecond);
    }
}

public interface IHasExecuteAction : ITransientService
{
    Action? OnExecute { get; set; }
}

public interface IUnitOfWorkScopedService : IUnitOfWorkScope, IHasExecuteAction
{
    Task DoWorkAsync();
}

internal sealed class UnitOfWorkScopedService : IUnitOfWorkScopedService
{
    public Action? OnExecute { get; set; }
    public Task DoWorkAsync()
    {
        OnExecute?.Invoke();
        return Task.CompletedTask;
    }
}

public interface IAttributedUnitOfWorkService : IHasExecuteAction
{
    Task DoWorkAsync();
    Task SkippedAsync();
    Task DoWorkOptionedAsync();
    Task GetAsync();
    Task GetOptionedAsync();
}

[UnitOfWork]
internal sealed class AttributedUnitOfWorkService : IAttributedUnitOfWorkService
{
    public Action? OnExecute { get; set; }

    public Task DoWorkAsync()
    {
        OnExecute?.Invoke();
        return Task.CompletedTask;
    }

    [UnitOfWork(false)]
    public Task SkippedAsync()
    {
        OnExecute?.Invoke();
        return Task.CompletedTask;
    }

    [UnitOfWork(UnitOfWorkTransactionBehavior.RequiresNew, IsolationLevel.Chaos, 5000)]
    public Task DoWorkOptionedAsync()
    {
        OnExecute?.Invoke();
        return Task.CompletedTask;
    }

    public Task GetAsync()
    {
        OnExecute?.Invoke();
        return Task.CompletedTask;
    }

    [UnitOfWork(UnitOfWorkTransactionBehavior.Required)]
    public Task GetOptionedAsync()
    {
        OnExecute?.Invoke();
        return Task.CompletedTask;
    }
}