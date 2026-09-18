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
    public async Task ShouldNotDetectChangeWhenEntityStateIsNotExplicitlySettedToModified()
    {
        // ExtraProperties is persisted as a JSON column. Without a custom ValueComparer,
        // EF Core cannot compare the contents of the dictionary — it falls back to reference
        // equality. As a result, modifying the dictionary in place will NOT be detected by automatic change tracking.
        //
        // This test demonstrates the failure mode: if we modify ExtraProperties and do NOT
        // explicitly set the entity state to Modified (e.g., by calling Repository.Update),
        // EF will leave the entity in the Unchanged state, and the changes will never be saved.
        //
        // Always call Repository.Update when modifying ExtraProperties.

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = new App2Entity1(Number);
            entity.SetProperty("key", "initial");

            await Repository.AddAsync(entity);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = await Repository.GetAsync(e => e.Number == Number);

            // Make change
            entity.SetProperty("key", "changed");

            // Intentionally DO NOT call Repository.Update or set the state to Modified

            var provider = fixture.Service<IDbContextProvider<App2DbContext>>();
            var dbContext = await provider.GetAsync();

            // Because the state was never explicitly set to Modified, EF change detection
            // does not recognize the modification to the JSON column. The entity remains Unchanged,
            // meaning SaveChanges would persist nothing.
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