using System;
using System.Collections.Generic;
using Allegory.Axiom.Data;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.EntityFrameworkCore.DbContexts;
using Allegory.Axiom.Extensibility;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore.ModelBuilding;

public class GlobalModelBuilderTests
{
    // Apply contributors

    [Fact]
    public void ShouldApplyContributorFromDbContext()
    {
        var dbContext = new HybridDbContext();
        var model = dbContext.Model;

        var annotation = model.FindAnnotation(HybridDbContextContributor.Annotation);

        annotation.ShouldNotBeNull();
        annotation.Value.ShouldBe(true);
    }

    [Fact]
    public void ShouldApplyContributorsFromReplacedDbContexts()
    {
        var dbContext = new HybridDbContext();
        var model = dbContext.Model;

        model.FindEntityType(typeof(Module1Entity1)).ShouldNotBeNull();
        model.FindEntityType(typeof(Module1Entity2)).ShouldNotBeNull();
        model.FindEntityType(typeof(Module2Entity1)).ShouldNotBeNull();
        model.FindEntityType(typeof(Module2Entity2)).ShouldNotBeNull();
        model.FindEntityType(typeof(Module3Entity1)).ShouldNotBeNull();
        model.FindEntityType(typeof(Module3Entity2)).ShouldNotBeNull();
    }

    [Fact]
    public void ShouldRespectTenancySideWhenApplyingContributorsFromReplacedDbContexts()
    {
        var hostDbContext = new HostSideDbContext();
        var hostModel = hostDbContext.Model;
        hostModel.FindEntityType(typeof(Module1Entity1)).ShouldNotBeNull();
        hostModel.FindEntityType(typeof(Module1Entity2)).ShouldNotBeNull();
        hostModel.FindEntityType(typeof(Module2Entity1)).ShouldBeNull();
        hostModel.FindEntityType(typeof(Module2Entity2)).ShouldBeNull();
        hostModel.FindEntityType(typeof(Module3Entity1)).ShouldNotBeNull();
        hostModel.FindEntityType(typeof(Module3Entity2)).ShouldBeNull();

        var tenantDbContext = new TenantSideDbContext();
        var tenantModel = tenantDbContext.Model;
        tenantModel.FindEntityType(typeof(Module1Entity1)).ShouldBeNull();
        tenantModel.FindEntityType(typeof(Module1Entity2)).ShouldBeNull();
        tenantModel.FindEntityType(typeof(Module2Entity1)).ShouldNotBeNull();
        tenantModel.FindEntityType(typeof(Module2Entity2)).ShouldNotBeNull();
        tenantModel.FindEntityType(typeof(Module3Entity1)).ShouldBeNull();
        tenantModel.FindEntityType(typeof(Module3Entity2)).ShouldNotBeNull();
    }

    // Configure conventions

    [Fact]
    public void ShouldConfigureCreationAuditedProperties()
    {
        var dbContext = new HybridDbContext();
        var model = dbContext.Model;

        var entityType = model.FindEntityType(typeof(Module2Entity1));
        entityType.ShouldNotBeNull();

        var createdAtProperty = entityType.FindProperty(nameof(ICreationAudited.CreatedAt));
        createdAtProperty.ShouldNotBeNull();
        createdAtProperty.ClrType.ShouldBe(typeof(DateTime));

        var createdByProperty = entityType.FindProperty(nameof(ICreationAudited.CreatedBy));
        createdByProperty.ShouldNotBeNull();
        createdByProperty.ClrType.ShouldBe(typeof(string));
        createdByProperty.GetMaxLength().ShouldBe(AuditingConstants.UserIdMaxLength);
    }

    [Fact]
    public void ShouldConfigureModificationAuditedProperties()
    {
        var dbContext = new HybridDbContext();
        var model = dbContext.Model;

        var entityType = model.FindEntityType(typeof(Module2Entity1));
        entityType.ShouldNotBeNull();

        var modifiedAtProperty = entityType.FindProperty(nameof(IModificationAudited.ModifiedAt));
        modifiedAtProperty.ShouldNotBeNull();
        modifiedAtProperty.ClrType.ShouldBe(typeof(DateTime?));

        var modifiedByProperty = entityType.FindProperty(nameof(IModificationAudited.ModifiedBy));
        modifiedByProperty.ShouldNotBeNull();
        modifiedByProperty.ClrType.ShouldBe(typeof(string));
        modifiedByProperty.GetMaxLength().ShouldBe(AuditingConstants.UserIdMaxLength);
    }

    [Fact]
    public void ShouldConfigureDeletionAuditedProperties()
    {
        var dbContext = new HybridDbContext();
        var model = dbContext.Model;

        var entityType = model.FindEntityType(typeof(Module2Entity1));
        entityType.ShouldNotBeNull();

        var deletedAtProperty = entityType.FindProperty(nameof(IDeletionAudited.DeletedAt));
        deletedAtProperty.ShouldNotBeNull();
        deletedAtProperty.ClrType.ShouldBe(typeof(DateTime?));

        var deletedByProperty = entityType.FindProperty(nameof(IDeletionAudited.DeletedBy));
        deletedByProperty.ShouldNotBeNull();
        deletedByProperty.ClrType.ShouldBe(typeof(string));
        deletedByProperty.GetMaxLength().ShouldBe(AuditingConstants.UserIdMaxLength);
    }

    [Fact]
    public void ShouldConfigureConcurrencyCheckProperty()
    {
        var dbContext = new HybridDbContext();
        var model = dbContext.Model;

        var entityType = model.FindEntityType(typeof(Module2Entity1));
        entityType.ShouldNotBeNull();

        var revisionProperty = entityType.FindProperty(nameof(IConcurrencyCheck.Revision));
        revisionProperty.ShouldNotBeNull();
        revisionProperty.IsConcurrencyToken.ShouldBeTrue();
    }

    [Fact]
    public void ShouldConfigureExtraProperties()
    {
        var dbContext = new HybridDbContext();
        var model = dbContext.Model;

        var entityType = model.FindEntityType(typeof(Module2Entity1));
        entityType.ShouldNotBeNull();

        var extraPropertiesProperty = entityType.FindProperty(nameof(IExtraProperties.ExtraProperties));
        extraPropertiesProperty.ShouldNotBeNull();
        extraPropertiesProperty.GetValueConverter().ShouldNotBeNull();
        extraPropertiesProperty.ClrType.ShouldBe(typeof(IDictionary<string, object>));
    }

    [Fact]
    public void ShouldConfigureSoftDeleteQueryFilter()
    {
        var dbContext = new HybridDbContext();
        var model = dbContext.Model;

        var entityType = model.FindEntityType(typeof(Module2Entity1));
        entityType.ShouldNotBeNull();

        var filters = entityType.GetDeclaredQueryFilters();
        filters.ShouldContain(e => e.Key == nameof(ISoftDelete));
    }

    [Fact]
    public void ShouldConfigureTenantOwnedQueryFilter()
    {
        var dbContext = new HybridDbContext();
        var model = dbContext.Model;

        var entityType = model.FindEntityType(typeof(Module2Entity1));
        entityType.ShouldNotBeNull();

        var filters = entityType.GetDeclaredQueryFilters();
        filters.ShouldContain(e => e.Key == nameof(ITenantOwned));
    }

    [Fact]
    public void ShouldCreateIndexForTenantOwnedAndSoftDelete()
    {
        var dbContext = new HybridDbContext();
        var model = dbContext.Model;

        var entityType = model.FindEntityType(typeof(Module2Entity1));
        entityType.ShouldNotBeNull();

        var index = entityType.FindIndex(
        [
            entityType.GetProperty(nameof(ITenantOwned.TenantId)),
            entityType.GetProperty(nameof(ISoftDelete.IsDeleted))
        ]);

        index.ShouldNotBeNull();
    }

    // Apply global contributors

    [Fact]
    public void ShouldApplyGlobalContributors()
    {
        var contributor = new GlobalContributor1();
        GlobalModelBuilder.Contributors.Add(contributor);

        try
        {
            var dbContext = new HybridDbContext();
            var model = dbContext.Model;

            var annotation = model.FindAnnotation(GlobalContributor1.Annotation);
            annotation.ShouldNotBeNull();
            annotation.Value.ShouldBe(true);
        }
        finally
        {
            GlobalModelBuilder.Contributors.Remove(contributor);
        }
    }
}

[ReplaceDbContext(typeof(Module1DbContext), typeof(Module2DbContext), typeof(Module3DbContext))]
file class HybridDbContext : DbContext, IModelBuilderContributorProvider
{
    public static IList<IModelBuilderContributor> Contributors { get; } = [new HybridDbContextContributor()];

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureAxiom(this);
    }
}

[TenancySide(TenancySide.Host)]
[ReplaceDbContext(typeof(Module1DbContext), typeof(Module2DbContext), typeof(Module3DbContext))]
file class HostSideDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureAxiom(this);
    }
}

[TenancySide(TenancySide.Tenant)]
[ReplaceDbContext(typeof(Module1DbContext), typeof(Module2DbContext), typeof(Module3DbContext))]
file class TenantSideDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureAxiom(this);
    }
}

file class HybridDbContextContributor : IModelBuilderContributor
{
    public const string Annotation = nameof(HybridDbContextContributor);

    public void Contribute(ModelBuilder modelBuilder, DbContext dbContext)
    {
        modelBuilder.Model.SetAnnotation(Annotation, true);
    }
}

file class GlobalContributor1 : IModelBuilderContributor
{
    public const string Annotation = nameof(GlobalContributor1);

    public void Contribute(ModelBuilder modelBuilder, DbContext dbContext)
    {
        modelBuilder.Model.SetAnnotation(Annotation, true);
    }
}