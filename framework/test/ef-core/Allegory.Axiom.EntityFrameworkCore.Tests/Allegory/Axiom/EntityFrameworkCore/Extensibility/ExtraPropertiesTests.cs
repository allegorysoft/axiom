using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.EntityFrameworkCore.DbContexts;
using Allegory.Axiom.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore.Extensibility;

public class ExtraPropertiesTests(ExtraPropertiesFixture fixture) : IClassFixture<ExtraPropertiesFixture>
{
    protected IRepository<App2Entity1, Guid> Repository => fixture.Service<IRepository<App2Entity1, Guid>>();
    protected string Number { get; } = Random.Shared.Next().ToString();

    [Fact]
    public async Task ShouldPersistExtraProperties()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = new App2Entity1(Number);
            entity.SetProperty("string", "value");
            entity.SetProperty("int", 42);
            entity.SetProperty("bool", true);
            entity.SetProperty("decimal", 12.34m);

            await Repository.AddAsync(entity);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = await Repository.GetAsync(e => e.Number == Number);

            entity.ExtraProperties.Values.All(x => x is JsonElement).ShouldBeTrue();
            entity.GetProperty<string>("string").ShouldBe("value");
            entity.GetProperty<int>("int").ShouldBe(42);
            entity.GetProperty<bool>("bool").ShouldBe(true);
            entity.GetProperty<decimal>("decimal").ShouldBe(12.34m);
        });
    }

    [Fact]
    public async Task ShouldHandleNullAndEmptyExtraProperties()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            // ExtraProperties is initialized as an empty dictionary
            var entity = new App2Entity1(Number);
            await Repository.AddAsync(entity);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = await Repository.GetAsync(e => e.Number == Number);

            entity.ExtraProperties.ShouldNotBeNull();
            entity.ExtraProperties.ShouldBeEmpty();
        });
    }

    [Fact]
    public async Task ShouldUpdateChangesWhenExtraPropertiesAreModified()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = new App2Entity1(Number);
            entity.SetProperty("key", "initial");

            await Repository.AddAsync(entity);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = await Repository.GetAsync(e => e.Number == Number);

            entity.GetProperty<string>("key").ShouldBe("initial");

            entity.SetProperty("key", "updated");
            entity.SetProperty("newKey", "newValue");

            await Repository.UpdateAsync(entity);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = await Repository.GetAsync(e => e.Number == Number);

            entity.GetProperty<string>("key").ShouldBe("updated");
            entity.GetProperty<string>("newKey").ShouldBe("newValue");
        });
    }

    [Fact]
    public async Task ShouldRemoveExtraPropertyWhenSetToNull()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = new App2Entity1(Number);
            entity.SetProperty("key", "initial");

            await Repository.AddAsync(entity);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = await Repository.GetAsync(e => e.Number == Number);

            entity.GetProperty<string>("key").ShouldBe("initial");

            entity.SetProperty("key", null); // Removes item from dictionary

            await Repository.UpdateAsync(entity);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = await Repository.GetAsync(e => e.Number == Number);

            entity.ExtraProperties.ShouldBeEmpty();
        });
    }

    [Fact]
    public async Task ShouldNotDetectChangeWhenExtraPropertiesAreReplacedWithEquivalentValues()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = new App2Entity1(Number);
            entity.SetProperty("key", "initial");
            entity.SetProperty("second", 2);

            await Repository.AddAsync(entity);
        });

        // The ValueComparer should treat this as no change, so no update should be persisted.
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = await Repository.GetAsync(e => e.Number == Number);

            entity.ExtraProperties.Clear();
            entity.SetProperty("key", "initial");
            entity.SetProperty("second", 2);

            var provider = fixture.Service<IDbContextProvider<App2DbContext>>();
            var dbContext = await provider.GetAsync();
            dbContext.Entity1.Entry(entity).State.ShouldBe(EntityState.Unchanged);
        });
    }
}

public class ExtraPropertiesFixture : IntegrationTest
{
    protected override async Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddAxiomDbContext<App2DbContext>(o =>
        {
            o.Configure(b => { b.UseSqlite("Data Source=ExtraProperties.db"); });

            o.Entity<App2Entity1>(e => { e.IncludeDetails = q => q.Include(n => n.SubEntities); });
        });
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await using var _ = BeginAutoCompletingUnitOfWork();

        var provider = Service<IDbContextProvider<App2DbContext>>();
        var dbContext = await provider.GetAsync();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
}