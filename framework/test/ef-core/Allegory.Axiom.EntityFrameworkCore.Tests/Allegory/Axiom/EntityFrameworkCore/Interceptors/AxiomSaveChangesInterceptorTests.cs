using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Entities.Events;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.EntityFrameworkCore.DbContexts;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.EventBus.Local;
using Allegory.Axiom.Security.Principal;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore.Interceptors;

public class AxiomSaveChangesInterceptorTests(
    AxiomSaveChangesInterceptorFixture fixture)
    : IClassFixture<AxiomSaveChangesInterceptorFixture>
{
    protected IRepository<App2Entity1, Guid> Repository => fixture.Service<IRepository<App2Entity1, Guid>>();
    protected string Number { get; } = Random.Shared.Next().ToString();
    protected string GetNewNumber => Random.Shared.Next().ToString();

    // Audit

    [Fact]
    public async Task ShouldSetCreationAudit()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = await Repository.AddAsync(new App2Entity1(Number), autoSave: true);

            entity.CreatedAt.ShouldBe(MockTimeProvider.Now.Date);
            entity.CreatedBy.ShouldBe(MockPrincipalAccessor.Principal.Identity!.FindNameIdentifier());
        });
    }

    [Fact]
    public async Task ShouldUseSpecifiedCreationAudit()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = new App2Entity1(Number);
            var createdAt = DateTime.UtcNow;
            const string createdBy = "john.doe";

            ObjectAccessor.TrySetProperty(entity, e => e.CreatedAt, createdAt);
            ObjectAccessor.TrySetProperty(entity, e => e.CreatedBy, createdBy);

            var result = await Repository.AddAsync(entity, autoSave: true);

            result.CreatedAt.ShouldBe(createdAt);
            result.CreatedBy.ShouldBe(createdBy);
        });
    }

    [Fact]
    public async Task ShouldSetModificationAudit()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = await Repository.AddAsync(new App2Entity1(Number), autoSave: true);

            entity.ModifiedAt.ShouldBeNull();
            entity.ModifiedBy.ShouldBeNull();

            entity.SetNumber(GetNewNumber);
            await Repository.UpdateAsync(entity, autoSave: true);

            entity.ModifiedAt.ShouldBe(MockTimeProvider.Now.Date);
            entity.ModifiedBy.ShouldBe(MockPrincipalAccessor.Principal.Identity!.FindNameIdentifier());
        });
    }

    [Fact]
    public async Task ShouldSetDeletionAudit()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = await Repository.AddAsync(new App2Entity1(Number), autoSave: true);

            entity.IsDeleted.ShouldBeFalse();
            entity.DeletedAt.ShouldBeNull();
            entity.DeletedBy.ShouldBeNull();

            await Repository.RemoveAsync(entity, autoSave: true);

            entity.IsDeleted.ShouldBeTrue();
            entity.DeletedAt.ShouldBe(MockTimeProvider.Now.Date);
            entity.DeletedBy.ShouldBe(MockPrincipalAccessor.Principal.Identity!.FindNameIdentifier());
        });
    }

    // Event publish

    [Fact]
    public async Task ShouldPublishAggregateEvents()
    {
        var entity = new App2Entity1(Number);

        await fixture.RunInUnitOfWorkAsync(async uow =>
        {
            var event1 = new Event1();
            var event2 = new Event2();

            entity.AddEvent(event1, isLocal: true);
            entity.AddEvent(event2, isLocal: false);

            entity = await Repository.AddAsync(entity);

            await uow.TryCompleteAsync(cancellationToken: CancellationToken.None);

            EventHandler.Received.ShouldContain(event1.Id);
            EventHandler.Received.ShouldContain(event2.Id);
        });

        await fixture.RunInUnitOfWorkAsync(async uow =>
        {
            var event1 = new Event1();
            var event2 = new Event2();

            entity.SetNumber(GetNewNumber);
            entity.AddEvent(event1, isLocal: true);
            entity.AddEvent(event2, isLocal: false);

            entity = await Repository.UpdateAsync(entity);

            await uow.TryCompleteAsync(cancellationToken: CancellationToken.None);

            EventHandler.Received.ShouldContain(event1.Id);
            EventHandler.Received.ShouldContain(event2.Id);
        });
    }

    [Fact]
    public async Task ShouldPublishEntityChangedEvents()
    {
        var entity = new App2Entity1(Number);

        await fixture.RunInUnitOfWorkAsync(async _ => { entity = await Repository.AddAsync(entity); });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            entity.SetNumber(GetNewNumber);
            entity = await Repository.UpdateAsync(entity);
        });

        await fixture.RunInUnitOfWorkAsync(async _ => { await Repository.RemoveAsync(entity); });

        EntityChangedHandler.Changed
            .Where(x => x.Item1 == entity.Id)
            .ShouldBe(
            [
                (entity.Id, EntityChangeType.Created),
                (entity.Id, EntityChangeType.Updated),
                (entity.Id, EntityChangeType.Deleted),
            ]);
    }

    [Fact]
    public async Task ShouldPublishEntityCreatedEvent()
    {
        var entity = new App2Entity1(Number);

        await fixture.RunInUnitOfWorkAsync(async _ => { entity = await Repository.AddAsync(entity); });

        EntityEventHandler.Changed.ShouldContain((entity.Id, EntityChangeType.Created));
    }

    [Fact]
    public async Task ShouldPublishEntityUpdatedEvent()
    {
        var entity = new App2Entity1(Number);

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            entity = await Repository.AddAsync(entity, autoSave: true);

            entity.SetNumber(GetNewNumber);
            await Repository.UpdateAsync(entity);
        });

        EntityEventHandler.Changed.ShouldContain((entity.Id, EntityChangeType.Updated));
    }

    [Fact]
    public async Task ShouldPublishEntityDeletedEvent()
    {
        var entity = new App2Entity1(Number);

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            entity = await Repository.AddAsync(entity, autoSave: true);
            await Repository.RemoveAsync(entity);
        });

        EntityEventHandler.Changed.ShouldContain((entity.Id, EntityChangeType.Deleted));
    }
}

public class AxiomSaveChangesInterceptorFixture : IntegrationTest
{
    protected override async Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddAxiomDbContext<App2DbContext>(o =>
        {
            o.Configure(b => { b.UseSqlite("Data Source=AxiomSaveChangesInterceptor.db"); });

            o.Entity<App2Entity1>(e => { e.IncludeDetails = q => q.Include(n => n.SubEntities); });
        });
    }

    protected override Task PostConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Replace(ServiceDescriptor.Singleton<TimeProvider, MockTimeProvider>());
        builder.Services.Replace(ServiceDescriptor.Singleton<IPrincipalAccessor, MockPrincipalAccessor>());

        return Task.CompletedTask;
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

file class MockTimeProvider : TimeProvider
{
    public static DateTimeOffset Now => new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => Now;
}

file class MockPrincipalAccessor : IPrincipalAccessor
{
    public static ClaimsPrincipal Principal { get; } = new(
        new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000000")]));

    public ClaimsPrincipal? Current => Principal;
}

file class Event1
{
    public Guid Id { get; } = Guid.NewGuid();
}

file class Event2
{
    public Guid Id { get; } = Guid.NewGuid();
}

file class EventHandler : ILocalEventHandler<Event1>, IDistributedEventHandler<Event2>
{
    public static List<Guid> Received { get; } = new();

    public Task HandleAsync(Event1 payload)
    {
        Received.Add(payload.Id);
        return Task.CompletedTask;
    }

    public Task HandleAsync(Event2 payload, EventContext context)
    {
        Received.Add(payload.Id);
        return Task.CompletedTask;
    }
}

file class EntityChangedHandler : ILocalEventHandler<EntityChanged<App2Entity1>>
{
    public static List<(Guid, EntityChangeType)> Changed { get; } = new();

    public Task HandleAsync(EntityChanged<App2Entity1> payload)
    {
        Changed.Add((payload.Entity.Id, payload.ChangeType));
        return Task.CompletedTask;
    }
}

file class EntityEventHandler :
    ILocalEventHandler<EntityCreated<App2Entity1>>,
    ILocalEventHandler<EntityUpdated<App2Entity1>>,
    ILocalEventHandler<EntityDeleted<App2Entity1>>
{
    public static List<(Guid, EntityChangeType)> Changed { get; } = new();

    public Task HandleAsync(EntityCreated<App2Entity1> payload)
    {
        Changed.Add((payload.Entity.Id, EntityChangeType.Created));
        return Task.CompletedTask;
    }

    public Task HandleAsync(EntityUpdated<App2Entity1> payload)
    {
        Changed.Add((payload.Entity.Id, EntityChangeType.Updated));
        return Task.CompletedTask;
    }

    public Task HandleAsync(EntityDeleted<App2Entity1> payload)
    {
        Changed.Add((payload.Entity.Id, EntityChangeType.Deleted));
        return Task.CompletedTask;
    }
}