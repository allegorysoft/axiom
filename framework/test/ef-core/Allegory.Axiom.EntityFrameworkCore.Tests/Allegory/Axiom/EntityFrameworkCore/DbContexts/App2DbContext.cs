using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Data;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.EntityFrameworkCore.Repositories;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore.DbContexts;

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

            builder.HasMany(x => x.SubEntities)
                .WithOne()
                .HasForeignKey(x => x.AppEntity1Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<App2SubEntity1>(builder =>
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.SubNumber)
                .IsRequired()
                .HasMaxLength(100);
        });

        // foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        // {
        //     var clrType = entityType.ClrType;
        //     var name = entityType.Name;
        // }
    }
}

public class App2Entity1 : AggregateRoot<Guid>, ICreationAudited, IModificationAudited, IDeletionAudited, ITenantOwned
{
    protected App2Entity1() { }

    public App2Entity1(string number, Guid? id = null)
    {
        Number = number;

        if (id != null)
        {
            Id = id.Value;
        }
    }

    public string Number { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Guid? TenantId { get; set; }

    public List<App2SubEntity1> SubEntities { get; set; } = [];
}

public class App2SubEntity1 : Entity<Guid>
{
    protected App2SubEntity1() { }

    public App2SubEntity1(string number, Guid? id = null)
    {
        SubNumber = number;

        if (id != null)
        {
            Id = id.Value;
        }
    }

    public Guid AppEntity1Id { get; set; }

    public string SubNumber { get; set; } = null!;
}