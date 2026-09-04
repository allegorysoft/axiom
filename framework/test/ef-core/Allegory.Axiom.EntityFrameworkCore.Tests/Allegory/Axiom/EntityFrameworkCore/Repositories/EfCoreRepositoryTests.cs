using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Data.Filtering;
using Allegory.Axiom.Data.IdGeneration;
using Allegory.Axiom.Domain;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.EntityFrameworkCore.DbContexts;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Testing.Platform.Services;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

public class EfCoreRepositoryTests(EfCoreRepositoryFixture fixture) : IClassFixture<EfCoreRepositoryFixture>
{
    protected IDbContextProvider<App2DbContext> DbContextProvider =>
        fixture.Service<IDbContextProvider<App2DbContext>>();

    protected IGuidGenerator GuidGenerator => fixture.Service<IGuidGenerator>();
    protected IRepository<App2Entity1, Guid> Repository => fixture.Service<IRepository<App2Entity1, Guid>>();

    protected string Number { get; } = Random.Shared.Next().ToString();
    protected string GetNewNumber => Random.Shared.Next().ToString();

    // Find

    [Fact]
    public async Task ShouldFindByPredicate()
    {
        await fixture.RunInUnitOfWorkAsync(async _ => { await Repository.AddAsync(new App2Entity1(Number)); });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.FindAsync(i => i.Number == Number);

            result.ShouldNotBeNull();
            result.Number.ShouldBe(Number);
        });
    }

    [Fact]
    public async Task ShouldFindById()
    {
        var id = GuidGenerator.Create();

        await fixture.RunInUnitOfWorkAsync(async _ => { await Repository.AddAsync(new App2Entity1(Number, id)); });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.FindAsync(id);

            result.ShouldNotBeNull();
            result.Number.ShouldBe(Number);
        });
    }

    [Fact]
    public async Task ShouldReturnNullWhenFindHasNoMatch()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.FindAsync(e => e.Number == "DOES-NOT-EXIST");

            result.ShouldBeNull();
        });
    }

    [Fact]
    public async Task ShouldFindWithOrWithoutDetails()
    {
        var id = GuidGenerator.Create();

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            await Repository.AddAsync(
                new App2Entity1(Number, id)
                {
                    SubEntities = new List<App2SubEntity1>
                    {
                        new("SUB-001"),
                        new("SUB-002"),
                        new("SUB-003")
                    }
                }
            );
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.FindAsync(id, includeDetails: true);

            result.ShouldNotBeNull();
            result.SubEntities.ShouldNotBeEmpty();
            result.SubEntities.Count.ShouldBe(3);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.FindAsync(id, includeDetails: false);

            result.ShouldNotBeNull();
            result.SubEntities.ShouldBeEmpty();
        });
    }

    // Get

    [Fact]
    public async Task ShouldGetByPredicate()
    {
        await fixture.RunInUnitOfWorkAsync(async _ => { await Repository.AddAsync(new App2Entity1(Number)); });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.GetAsync(i => i.Number == Number);

            result.ShouldNotBeNull();
            result.Number.ShouldBe(Number);
        });
    }

    [Fact]
    public async Task ShouldGetById()
    {
        var id = GuidGenerator.Create();

        await fixture.RunInUnitOfWorkAsync(async _ => { await Repository.AddAsync(new App2Entity1(Number, id)); });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.GetAsync(id);

            result.ShouldNotBeNull();
            result.Number.ShouldBe(Number);
        });
    }

    [Fact]
    public async Task ShouldThrowEntityNotFoundExceptionWhenGetHasNoMatch()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var exception = await Should.ThrowAsync<EntityNotFoundException>(async () =>
            {
                await Repository.GetAsync(e => e.Number == "DOES-NOT-EXIST");
            });

            exception.Code.ShouldBe(DomainExceptionCodes.EntityNotFound);
        });
    }

    [Fact]
    public async Task ShouldGetWithOrWithoutDetails()
    {
        var id = GuidGenerator.Create();

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            await Repository.AddAsync(
                new App2Entity1(Number, id)
                {
                    SubEntities = new List<App2SubEntity1>
                    {
                        new("SUB-001"),
                        new("SUB-002"),
                        new("SUB-003")
                    }
                }
            );
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.GetAsync(id, includeDetails: true);

            result.ShouldNotBeNull();
            result.SubEntities.ShouldNotBeEmpty();
            result.SubEntities.Count.ShouldBe(3);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.GetAsync(id, includeDetails: false);

            result.ShouldNotBeNull();
            result.SubEntities.ShouldBeEmpty();
        });
    }

    // GetList

    [Fact]
    public async Task ShouldGetList()
    {
        IReadOnlyList<App2Entity1> before = null!;

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            before = await Repository.GetListAsync();

            await Repository.AddRangeAsync([new App2Entity1(GetNewNumber), new App2Entity1(GetNewNumber)]);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var after = await Repository.GetListAsync();
            after.Count.ShouldBe(before.Count + 2);
        });
    }

    [Fact]
    public async Task ShouldGetListByPredicate()
    {
        var list = new List<string>
        {
            GetNewNumber,
            GetNewNumber
        };

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            await Repository.AddRangeAsync(
            [
                new App2Entity1(list[0]),
                new App2Entity1(list[1]),
                new App2Entity1(GetNewNumber)
            ]);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var results = await Repository.GetListAsync(e => list.Contains(e.Number));

            results.Count.ShouldBe(2);
        });
    }

    [Fact]
    public async Task ShouldGetListInSpecifiedOrder()
    {
        var list = new List<string>
        {
            GetNewNumber,
            GetNewNumber,
            GetNewNumber,
        };

        var orderedList = list.OrderBy(e => e);

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            foreach (var item in list)
            {
                await Repository.AddAsync(new App2Entity1(item));
            }
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var results = await Repository.GetListAsync(
                e => list.Contains(e.Number),
                orderBy: q => q.OrderBy(e => e.Number));

            results.Select(e => e.Number).ShouldBe(orderedList);
        });
    }

    [Fact]
    public async Task ShouldGetListWithOrWithoutDetails()
    {
        var list = new List<string>
        {
            GetNewNumber,
            GetNewNumber,
            GetNewNumber,
        };

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            foreach (var item in list)
            {
                await Repository.AddAsync(new App2Entity1(item)
                {
                    SubEntities = new List<App2SubEntity1>
                    {
                        new(GetNewNumber),
                        new(GetNewNumber),
                        new(GetNewNumber)
                    }
                });
            }
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.GetListAsync(e => list.Contains(e.Number), includeDetails: true);
            foreach (var item in result)
            {
                item.ShouldNotBeNull();
                item.SubEntities.ShouldNotBeEmpty();
                item.SubEntities.Count.ShouldBe(3);
            }
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.GetListAsync(e => list.Contains(e.Number), includeDetails: false);
            foreach (var item in result)
            {
                item.ShouldNotBeNull();
                item.SubEntities.ShouldBeEmpty();
            }
        });
    }

    // GetPagedList

    [Fact]
    public async Task ShouldGetPagedList()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            for (var i = 0; i < 10; i++)
            {
                await Repository.AddAsync(new App2Entity1(GetNewNumber));
            }
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.GetPagedListAsync(0, 5);
            result.Count.ShouldBe(5);
        });
    }

    [Fact]
    public async Task ShouldGetPagedListByPredicate()
    {
        var list = new List<string>
        {
            GetNewNumber,
            GetNewNumber
        };

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            await Repository.AddRangeAsync(
            [
                new App2Entity1(list[0]),
                new App2Entity1(list[1]),
                new App2Entity1(GetNewNumber)
            ]);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var results = await Repository.GetPagedListAsync(0, 10, e => list.Contains(e.Number));

            results.Count.ShouldBe(2);
        });
    }

    [Fact]
    public async Task ShouldGetPagedListInSpecifiedOrder()
    {
        var list = new List<string>
        {
            GetNewNumber,
            GetNewNumber,
            GetNewNumber,
        };

        var orderedList = list.OrderBy(e => e);

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            foreach (var item in list)
            {
                await Repository.AddAsync(new App2Entity1(item));
            }
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var results = await Repository.GetPagedListAsync(
                0,
                10,
                predicate: e => list.Contains(e.Number),
                orderBy: q => q.OrderBy(e => e.Number));

            results.Select(e => e.Number).ShouldBe(orderedList);
        });
    }

    [Fact]
    public async Task ShouldGetPagedListWithOrWithoutDetails()
    {
        var list = new List<string>
        {
            GetNewNumber,
            GetNewNumber,
            GetNewNumber,
        };

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            foreach (var item in list)
            {
                await Repository.AddAsync(new App2Entity1(item)
                {
                    SubEntities = new List<App2SubEntity1>
                    {
                        new(GetNewNumber),
                        new(GetNewNumber),
                        new(GetNewNumber)
                    }
                });
            }
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.GetPagedListAsync(0, 10, e => list.Contains(e.Number), includeDetails: true);
            foreach (var item in result)
            {
                item.ShouldNotBeNull();
                item.SubEntities.ShouldNotBeEmpty();
                item.SubEntities.Count.ShouldBe(3);
            }
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.GetPagedListAsync(0, 10, e => list.Contains(e.Number), includeDetails: false);
            foreach (var item in result)
            {
                item.ShouldNotBeNull();
                item.SubEntities.ShouldBeEmpty();
            }
        });
    }

    // GetCount

    [Fact]
    public async Task ShouldGetCount()
    {
        long before = 0;

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            before = await Repository.GetCountAsync();

            await Repository.AddRangeAsync([new App2Entity1(GetNewNumber), new App2Entity1(GetNewNumber)]);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var after = await Repository.GetCountAsync();

            after.ShouldBe(before + 2);
        });
    }

    [Fact]
    public async Task ShouldGetCountByPredicate()
    {
        var list = new List<string>
        {
            GetNewNumber,
            GetNewNumber
        };

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            await Repository.AddRangeAsync(
            [
                new App2Entity1(list[0]),
                new App2Entity1(list[1]),
                new App2Entity1(GetNewNumber)
            ]);
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.GetCountAsync(e => list.Contains(e.Number));

            result.ShouldBe(2);
        });
    }

    // Add

    [Fact]
    public async Task ShouldAdd()
    {
        var entity = new App2Entity1(GetNewNumber)
        {
            SubEntities = new List<App2SubEntity1>
            {
                new(GetNewNumber),
                new(GetNewNumber),
            }
        };

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            await Repository.AddAsync(entity);

            // autoSave: false
            var result = await Repository.FindAsync(entity.Id);
            result.ShouldBeNull();
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.GetAsync(entity.Id);

            result.ShouldNotBeNull();
            result.Number.ShouldBe(entity.Number);
            result.SubEntities.Count.ShouldBe(2);
        });
    }

    [Fact]
    public async Task ShouldAddWithAutoSave()
    {
        var entity = new App2Entity1(GetNewNumber)
        {
            SubEntities = new List<App2SubEntity1>
            {
                new(GetNewNumber),
                new(GetNewNumber),
            }
        };

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            await Repository.AddAsync(entity, autoSave: true);

            var result = await Repository.GetAsync(entity.Id);
            result.ShouldNotBeNull();
            result.Number.ShouldBe(entity.Number);
            result.SubEntities.Count.ShouldBe(2);
        });
    }

    [Fact]
    public async Task ShouldAddRange()
    {
        var entities = new List<App2Entity1>
        {
            new(GetNewNumber)
            {
                SubEntities = new List<App2SubEntity1>
                {
                    new(GetNewNumber),
                    new(GetNewNumber),
                }
            },
            new(GetNewNumber)
            {
                SubEntities = new List<App2SubEntity1>
                {
                    new(GetNewNumber),
                    new(GetNewNumber),
                }
            },
            new(GetNewNumber)
            {
                SubEntities = new List<App2SubEntity1>
                {
                    new(GetNewNumber),
                    new(GetNewNumber),
                }
            }
        };

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            await Repository.AddRangeAsync(entities);

            // autoSave: false
            var result = await Repository.GetListAsync(e => entities.Contains(e));
            result.ShouldBeEmpty();
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.GetListAsync(e => entities.Contains(e), includeDetails: true);


            result.ShouldNotBeNull();
            result.Count.ShouldBe(entities.Count);
            result.SelectMany(e => e.SubEntities).Count().ShouldBe(entities.SelectMany(e => e.SubEntities).Count());
        });
    }

    [Fact]
    public async Task ShouldAddRangeWithAutoSave()
    {
        var entities = new List<App2Entity1>
        {
            new(GetNewNumber)
            {
                SubEntities = new List<App2SubEntity1>
                {
                    new(GetNewNumber),
                    new(GetNewNumber),
                }
            },
            new(GetNewNumber)
            {
                SubEntities = new List<App2SubEntity1>
                {
                    new(GetNewNumber),
                    new(GetNewNumber),
                }
            },
            new(GetNewNumber)
            {
                SubEntities = new List<App2SubEntity1>
                {
                    new(GetNewNumber),
                    new(GetNewNumber),
                }
            }
        };

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            await Repository.AddRangeAsync(entities, autoSave: true);

            var result = await Repository.GetListAsync(e => entities.Contains(e), includeDetails: true);

            result.ShouldNotBeNull();
            result.Count.ShouldBe(entities.Count);
            result.SelectMany(e => e.SubEntities).Count().ShouldBe(entities.SelectMany(e => e.SubEntities).Count());
        });
    }

    // Update

    [Fact]
    public async Task ShouldUpdate()
    {
        await fixture.RunInUnitOfWorkAsync(async _ => { await Repository.AddAsync(new App2Entity1(Number)); });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = await Repository.GetAsync(e => e.Number == Number);

            entity.SubEntities.Count.ShouldBe(0);

            entity.SubEntities.Add(new App2SubEntity1(GetNewNumber));
            entity.SubEntities.Add(new App2SubEntity1(GetNewNumber));

            await Repository.UpdateAsync(entity);

            // Same ambient DbContext throughout this unit of work.
            var context = await DbContextProvider.GetAsync();

            // autoSave: false means UpdateAsync only marks the entity as
            // Modified in the change tracker it does not call SaveChangesAsync,
            // so no SQL has been sent and no transaction has been opened yet.
            context.Database.CurrentTransaction.ShouldBeNull();
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var result = await Repository.GetAsync(e => e.Number == Number);
            result.SubEntities.Count.ShouldBe(2);
        });
    }

    [Fact]
    public async Task ShouldUpdateWithAutoSave()
    {
        await fixture.RunInUnitOfWorkAsync(async _ => { await Repository.AddAsync(new App2Entity1(Number)); });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entity = await Repository.GetAsync(e => e.Number == Number);

            entity.SubEntities.Count.ShouldBe(0);

            entity.SubEntities.Add(new App2SubEntity1(GetNewNumber));
            entity.SubEntities.Add(new App2SubEntity1(GetNewNumber));

            await Repository.UpdateAsync(entity, autoSave: true);
            
            var result = await Repository.GetAsync(e => e.Number == Number);
            result.SubEntities.Count.ShouldBe(2);

            var context = await DbContextProvider.GetAsync();

            // autoSave: true forces UpdateAsync to call SaveChangesAsync
            // immediately. EF opens an implicit transaction the moment it
            // executes SQL against a relational provider, so a non-null
            // CurrentTransaction here proves the save actually happened
            // eagerly, inside UpdateAsync not deferred to UoW completion.
            context.Database.CurrentTransaction.ShouldNotBeNull();
        });
    }

    [Fact]
    public async Task ShouldUpdateRange()
    {
        var numbers = new List<string>
        {
            GetNewNumber,
            GetNewNumber,
            GetNewNumber
        };

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            foreach (var number in numbers)
            {
                await Repository.AddAsync(new App2Entity1(number));
            }
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entities = await Repository.GetListAsync(e => numbers.Contains(e.Number), includeDetails: true);

            foreach (var entity in entities)
            {
                entity.SubEntities.Count.ShouldBe(0);

                entity.SubEntities.Add(new App2SubEntity1(GetNewNumber));
                entity.SubEntities.Add(new App2SubEntity1(GetNewNumber));
            }

            await Repository.UpdateRangeAsync(entities);

            // Same ambient DbContext throughout this unit of work.
            var context = await DbContextProvider.GetAsync();

            // autoSave: false means UpdateAsync only marks the entity as
            // Modified in the change tracker it does not call SaveChangesAsync,
            // so no SQL has been sent and no transaction has been opened yet.
            context.Database.CurrentTransaction.ShouldBeNull();
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entities = await Repository.GetListAsync(e => numbers.Contains(e.Number), includeDetails: true);

            entities.ShouldNotBeNull();
            entities.Count.ShouldBe(numbers.Count);
            // We add 2 sub item for each aggregate
            entities.SelectMany(e => e.SubEntities).Count().ShouldBe(numbers.Count * 2);
        });
    }

    [Fact]
    public async Task ShouldUpdateRangeWithAutoSave()
    {
        var r = Repository;
        var numbers = new List<string>
        {
            GetNewNumber,
            GetNewNumber,
            GetNewNumber
        };

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            foreach (var number in numbers)
            {
                await Repository.AddAsync(new App2Entity1(number));
            }
        });

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var entities = await Repository.GetListAsync(e => numbers.Contains(e.Number), includeDetails: true);

            foreach (var entity in entities)
            {
                entity.SubEntities.Count.ShouldBe(0);

                entity.SubEntities.Add(new App2SubEntity1(GetNewNumber));
                entity.SubEntities.Add(new App2SubEntity1(GetNewNumber));
            }

            await Repository.UpdateRangeAsync(entities, autoSave: true);

            var result = await Repository.GetListAsync(e => numbers.Contains(e.Number), includeDetails: true);
            result.ShouldNotBeNull();
            result.Count.ShouldBe(numbers.Count);
            // We add 2 sub item for each aggregate
            result.SelectMany(e => e.SubEntities).Count().ShouldBe(numbers.Count * 2);

            var context = await DbContextProvider.GetAsync();

            // autoSave: true forces UpdateAsync to call SaveChangesAsync
            // immediately. EF opens an implicit transaction the moment it
            // executes SQL against a relational provider, so a non-null
            // CurrentTransaction here proves the save actually happened
            // eagerly, inside UpdateAsync not deferred to UoW completion.
            context.Database.CurrentTransaction.ShouldNotBeNull();
        });
    }

    [Fact]
    public async Task ShouldRemove()
    {
        var entity = new App2Entity1(GetNewNumber)
        {
            SubEntities = new List<App2SubEntity1>
            {
                new(GetNewNumber),
                new(GetNewNumber),
            }
        };

        var filterSwitch = fixture.Service<IFilterSwitch>();
    }

    protected async Task T()
    {
        var filterSwitch = fixture.Service<IFilterSwitch>();

        var f = filterSwitch.IsEnabled<ITenantOwned>();

        var a = filterSwitch.IsEnabled<ITenantOwned>();
    }
    
    // RemoveById

    // RemoveRange
    // RemoveRangeById

    // Soft delete
    // Hard delete
}

public class EfCoreRepositoryFixture : IntegrationTest
{
    protected override async Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        var container = new PostgreSqlBuilder("postgres:latest")
            .WithUsername("admin")
            .WithPassword("admin")
            .Build();

        await builder.AddTestContainerAsync(container);

        builder.Services.AddAxiomDbContext<App2DbContext>(o =>
        {
            o.Configure(b => { b.UseNpgsql(container.GetConnectionString()); });

            o.Entity<App2Entity1>(e => { e.IncludeDetails = q => q.Include(n => n.SubEntities); });
        });
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await using var _ = BeginAutoCompletingUnitOfWork();

        var provider = Host.Services.GetRequiredService<IDbContextProvider<App2DbContext>>();
        var dbContext = await provider.GetAsync();
        await dbContext.Database.MigrateAsync();
    }
}