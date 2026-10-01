using System;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.DistributedLocking;

public class DistributedLockBaseTests(DistributedLockBaseFixture fixture) : IClassFixture<DistributedLockBaseFixture>
{
    public DistributedLockImp DistributedLock => (DistributedLockImp)fixture.Service<IDistributedLock>();

    [Fact]
    public async Task ShouldPassKeyToCore()
    {
        await DistributedLock.TryAcquireAsync("key", cancellationToken: CancellationToken.None);

        DistributedLock.LastKey.ShouldBe("key");
    }

    [Fact]
    public async Task ShouldAddKeyPrefixWhenSpecified()
    {
        var provider = await fixture.CreateServiceProviderAsync(async builder =>
        {
            await fixture.Configure(builder);

            builder.Services.Configure<DistributedLockOptions>(options =>
            {
                options.KeyPrefix = "app:";
            });
        });

        var distributedLock = provider.GetRequiredService<IDistributedLock>();

        await distributedLock.TryAcquireAsync("key", cancellationToken: CancellationToken.None);

        ((DistributedLockImp) distributedLock).LastKey.ShouldBe("app:key");
    }

    [Fact]
    public async Task ShouldPassTimeoutToCore()
    {
        var timeout = TimeSpan.FromSeconds(10);

        await DistributedLock.TryAcquireAsync("key", timeout, cancellationToken: CancellationToken.None);

        DistributedLock.LastTimeout.ShouldBe(timeout);
    }

    [Fact]
    public async Task ShouldPassCancellationTokenToCore()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        await DistributedLock.TryAcquireAsync(
            "key",
            cancellationToken: cancellationTokenSource.Token);

        DistributedLock.LastCancellationToken.ShouldBe(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task ShouldUseUnitOfWorkCancellationTokenWhenCancellationTokenNotSpecified()
    {
        var uowManager = fixture.Service<IUnitOfWorkManager>();

        await using var uow = uowManager.Begin(cancellationToken: TestContext.Current.CancellationToken);

        await DistributedLock.TryAcquireAsync("key", cancellationToken: CancellationToken.None);

        DistributedLock.LastCancellationToken.ShouldBe(TestContext.Current.CancellationToken);
    }
}

public class DistributedLockBaseFixture : IntegrationTest
{
    public Task Configure(IHostApplicationBuilder builder) => ConfigureAsync(builder);

    protected override Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<IDistributedLock, DistributedLockImp>());

        return Task.CompletedTask;
    }
}

[Dependency(AutoRegister = false)]
public class DistributedLockImp(
    IOptions<DistributedLockOptions> options,
    IUnitOfWorkManager unitOfWorkManager) :
    DistributedLockBase(options, unitOfWorkManager)
{
    public string? LastKey { get; private set; }
    public TimeSpan LastTimeout { get; private set; }
    public CancellationToken LastCancellationToken { get; private set; }

    protected override ValueTask<IAsyncDisposable?> TryAcquireCoreAsync(
        string key,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default)
    {
        LastKey = key;
        LastTimeout = timeout;
        LastCancellationToken = cancellationToken;

        return ValueTask.FromResult<IAsyncDisposable?>(new TestLockHandle());
    }

    private sealed class TestLockHandle : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}