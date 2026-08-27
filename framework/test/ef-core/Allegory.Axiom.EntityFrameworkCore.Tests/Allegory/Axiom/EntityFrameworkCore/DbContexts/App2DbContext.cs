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

public class App2Entity1 : AggregateRoot<int>, ICreationAudited, IModificationAudited, IDeletionAudited, ITenantOwned
{
    protected App2Entity1() { }

    public App2Entity1(string number)
    {
        Number = number;
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

public class App2SubEntity1 : Entity<int>
{
    public int AppEntity1Id { get; set; }

    public string SubNumber { get; set; } = null!;
}

public interface IApp2Entity1Repository : IRepository<App2Entity1, int>
{
    ValueTask<IQueryable<App2Entity1>> GetQueryable();
}

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

    public async ValueTask<IQueryable<App2Entity1>> GetQueryable()
    {
        var set = await GetDbSetAsync();
        var query = set.AsNoTracking().AsQueryable();
        return query;
    }
}