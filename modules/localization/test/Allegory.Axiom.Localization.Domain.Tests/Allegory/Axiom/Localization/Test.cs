using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Caching;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Caching.Hybrid;
using Xunit;

namespace Allegory.Axiom.Localization;

public class Test(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task T()
    {
        var cache = fixture.Service<ICache>();
        var hybridCache = fixture.Service<HybridCache>();
        var unitOfWorkManager = fixture.Service<IUnitOfWorkManager>();
        var repository = fixture.Service<ILocalizationRecordRepository>();

        var r = await hybridCache.GetOrCreateAsync(
            "localizations:resources",
            async ct =>
            {
                await using var uow = unitOfWorkManager.Begin(
                    cancellationToken: ct,
                    options: UnitOfWorkOptions.Suppress);

                var resources = await repository.GetResourceListAsync(ct);

                return new LocalizationResourcesCacheItem()
                {
                    Items = resources
                };
            },
            new HybridCacheEntryOptions {Expiration = TimeSpan.FromHours(1)},
            cancellationToken: TestContext.Current.CancellationToken);
        
        var r2 = await hybridCache.GetOrCreateAsync(
            "localizations:resources",
            async ct =>
            {
                await using var uow = unitOfWorkManager.Begin(
                    cancellationToken: ct,
                    options: UnitOfWorkOptions.Suppress);

                var resources = await repository.GetResourceListAsync(ct);

                return new LocalizationResourcesCacheItem()
                {
                    Items = resources
                };
            },
            new HybridCacheEntryOptions {Expiration = TimeSpan.FromHours(1)},
            cancellationToken: TestContext.Current.CancellationToken);

        if (r == r2)
        {
            
        }
    }
}