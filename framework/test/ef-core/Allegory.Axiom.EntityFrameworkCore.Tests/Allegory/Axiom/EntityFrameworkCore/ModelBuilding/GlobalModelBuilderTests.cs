using System.Collections.Generic;
using Allegory.Axiom.EntityFrameworkCore.DbContexts;
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
        model.FindEntityType(typeof(Module1Entity2)).ShouldNotBeNull();
        model.FindEntityType(typeof(Module3Entity1)).ShouldNotBeNull();
        model.FindEntityType(typeof(Module1Entity2)).ShouldNotBeNull();
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

    // Apply global contributors
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