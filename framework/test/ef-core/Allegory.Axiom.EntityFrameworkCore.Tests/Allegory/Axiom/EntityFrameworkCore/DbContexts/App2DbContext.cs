using System;
using System.Collections.Generic;
using System.Linq;
using Allegory.Axiom.Data;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.EntityFrameworkCore.Repositories;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore.DbContexts;

[ConnectionStringName("App2")]
public class App2DbContext(DbContextOptions<App2DbContext> options) : DbContext(options)
{
    public DbSet<App2Entity1> Entity1 => Set<App2Entity1>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<App2Entity1>(builder =>
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Number)
                .IsRequired()
                .HasMaxLength(100);

            // Soft delete global query filter
            // builder.HasQueryFilter(e => !e.IsDeleted);

            // Configure owned or child collection
            // builder.HasMany(x => x.SubEntities)
            //     .WithOne()
            //     .HasForeignKey(x => x.AppEntity1Id)
            //     .OnDelete(DeleteBehavior.Cascade);

            // Configure as an Owned Collection with explicit Primary Key
            builder.OwnsMany(x => x.SubEntities, subBuilder =>
            {
                subBuilder.WithOwner().HasForeignKey(x => x.Entity1Id);

                subBuilder.Property(e => e.Id);
                subBuilder.HasKey(e => e.Id);

                subBuilder.Property(e => e.SubNumber)
                    .IsRequired()
                    .HasMaxLength(100);
            });
        });
    }
}

public class App2Entity1 : AggregateRoot<int>, ICreationAudited, IModificationAudited, IDeletionAudited, ITenantOwned
{
    protected App2Entity1() { }

    public App2Entity1(string number)
    {
        Number = number;
    }

    public string Number { get; set; } = null!;

    public DateTime CreatedAt { get; private init; }
    public string? CreatedBy { get; private init; }

    public DateTime? ModifiedAt { get; private init; }
    public string? ModifiedBy { get; private init; }

    public bool IsDeleted { get; private init; }
    public DateTime? DeletedAt { get; private init; }
    public string? DeletedBy { get; private init; }

    public Guid? TenantId { get; private init; }

    public List<App2SubEntity1> SubEntities { get; } = [];
}

public class App2SubEntity1 : Entity<int>
{
    public int Entity1Id { get; set; }

    public string SubNumber { get; set; } = null!;
}

public interface IApp2Entity1Repository : IRepository<App2Entity1, int> { }

public class EfCoreEntity1Repository(
    IDbContextProvider<App2DbContext> dbContextProvider)
    : EfCoreRepository<App2DbContext, App2Entity1, int>(dbContextProvider), IApp2Entity1Repository
{
    protected override IQueryable<App2Entity1> IncludeDetails(
        IQueryable<App2Entity1> query,
        bool includeDetails = true)
    {
        return query.Include(q => q.SubEntities);
    }
}