using System;
using System.Collections.Generic;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Entities.Auditing;
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
                .HasMaxLength(App2Entity1.MaxNumberLength);

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
                .HasMaxLength(App2SubEntity1.MaxNumberLength);
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
    public static byte MaxNumberLength { get; set; } = 100;

    protected App2Entity1() { }

    public App2Entity1(string number, Guid? id = null)
    {
        SetNumber(number);

        if (id != null)
        {
            Id = id.Value;
        }
    }

    public string Number { get; protected set; } = null!;

    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }

    public DateTime? ModifiedAt { get; private set; }
    public string? ModifiedBy { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public Guid? TenantId { get; private set; }

    public List<App2SubEntity1> SubEntities { get; set; } = [];

    public void SetNumber(string number)
    {
        ArgumentNullException.ThrowIfNull(number);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(number.Length, MaxNumberLength);

        Number = number;
    }
}

public class App2SubEntity1 : Entity<Guid>
{
    public static byte MaxNumberLength { get; set; } = 100;

    protected App2SubEntity1() { }

    public App2SubEntity1(string number, Guid? id = null)
    {
        SetSubNumber(number);

        if (id != null)
        {
            Id = id.Value;
        }
    }

    public Guid AppEntity1Id { get; private init; }

    public string SubNumber { get; protected set; } = null!;

    public void SetSubNumber(string number)
    {
        ArgumentNullException.ThrowIfNull(number);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(number.Length, MaxNumberLength);

        SubNumber = number;
    }
}