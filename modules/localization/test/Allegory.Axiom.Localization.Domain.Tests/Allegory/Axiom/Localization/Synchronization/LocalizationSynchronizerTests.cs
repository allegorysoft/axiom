using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Localization;

public class LocalizationSynchronizerTests(
    IntegrationTestFixture fixture) :
    IClassFixture<IntegrationTestFixture>
{
    protected ILocalizationRecordRepository Repository => fixture.Service<ILocalizationRecordRepository>();
    protected LocalizationSynchronizer Synchronizer => fixture.Service<LocalizationSynchronizer>();
    protected ILocalizationProvider Provider => fixture.Service<ILocalizationProvider>();

    // Each test method gets new resource name
    protected string Resource { get; } = Random.Shared.Next().ToString();

    [Fact]
    public async Task ShouldSaveResourcesAndTranslationsOnSynchronizeAsync()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            // Clean up existing resources that may have been created by other tests.
            var resourcesBefore = await Repository.GetListAsync();
            await Repository.RemoveRangeAsync(resourcesBefore);
        });
        
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var resourcesBefore = await Repository.GetResourceListAsync();
            resourcesBefore.ShouldBeEmpty();
        });

        var context = new LocalizationContext
        {
            DefaultCulture = "en",
            Translations = new ConcurrentDictionary<string, FrozenDictionary<string, string>>
            {
                ["en"] = new Dictionary<string, string>
                    {
                        ["Hello"] = "Hello", ["Welcome"] = "Welcome"
                    }
                    .ToFrozenDictionary(),
                ["tr"] = new Dictionary<string, string>
                    {
                        ["Hello"] = "Merhaba", ["Welcome"] = "Hoş geldiniz"
                    }
                    .ToFrozenDictionary(),
            }
        };
        Provider.Contexts.TryAdd(Resource, context);

        await Synchronizer.SynchronizeAsync(TestContext.Current.CancellationToken);

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var resourcesAfter = await Repository.GetResourceListAsync(CancellationToken.None);

            var resource = resourcesAfter.Single(r => r.Name == Resource);
            resource.Name.ShouldBe(Resource);
            resource.DefaultCulture.ShouldBe("en");
            resource.Cultures.ShouldBe(["en", "tr"], ignoreOrder: true);
        });
    }

    [Fact]
    public async Task ShouldUpdateExistingTranslationsOnReSynchronize()
    {
        var context = new LocalizationContext
        {
            DefaultCulture = "en",
            Translations = new ConcurrentDictionary<string, FrozenDictionary<string, string>>
            {
                ["en"] = new Dictionary<string, string> {["Key1"] = "Value1"}.ToFrozenDictionary(),
            }
        };
        Provider.Contexts.TryAdd(Resource, context);
        await Synchronizer.SynchronizeAsync(TestContext.Current.CancellationToken);

        context.Translations["en"] = new Dictionary<string, string>
        {
            ["Key1"] = "UpdatedValue1", ["Key2"] = "NewValue"
        }.ToFrozenDictionary();
        await Synchronizer.SynchronizeAsync(TestContext.Current.CancellationToken);

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var record = await Repository.FindAsync(r => r.Name == Resource && r.Culture == "en");
            record.ShouldNotBeNull();
            record.Translations["Key1"].ShouldBe("UpdatedValue1");
            record.Translations["Key2"].ShouldBe("NewValue");
        });
    }

    [Fact]
    public async Task ShouldReloadContextsFromDatabaseOnReloadAsync()
    {
        Provider.Contexts.TryAdd(Resource, new LocalizationContext
        {
            DefaultCulture = "en",
            Translations = new ConcurrentDictionary<string, FrozenDictionary<string, string>>
            {
                ["en"] = new Dictionary<string, string> {["Key"] = "Value"}.ToFrozenDictionary(),
            }
        });
        await Synchronizer.SynchronizeAsync(TestContext.Current.CancellationToken);

        Provider.Contexts.Clear();
        await Synchronizer.ReloadAsync(TestContext.Current.CancellationToken);

        Provider.Contexts.ShouldContainKey(Resource);
        var reloaded = Provider.Contexts[Resource];
        reloaded.DefaultCulture.ShouldBe("en");
        reloaded.Translations["en"]["Key"].ShouldBe("Value");
    }
}