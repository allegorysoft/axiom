using System;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Data.ConnectionStrings;
using Allegory.Axiom.EntityFrameworkCore.DbContexts;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore;

public class EntityFrameworkCorePackageTests : IntegrationTest
{
    public override ValueTask InitializeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ShouldConfigureAxiomDbContextsOptions()
    {
        var provider = await CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<App1DbContext>();
                builder.Services.AddAxiomDbContext<Module1DbContext>(o => o.RegisterAsGenericDbContext = true);
            });

        var options = provider.GetRequiredService<IOptions<AxiomDbContextsOptions>>().Value;

        options.Contexts.ShouldContain(typeof(App1DbContext));
        options.Contexts.ShouldContain(typeof(Module1DbContext));
    }

    [Fact]
    public async Task ShouldConfigureAxiomDbContextOptions()
    {
        var appBuilderAction = (IServiceProvider sp, DbContextOptionsBuilder b) => { };
        var moduleBuilderAction = (IServiceProvider sp, DbContextOptionsBuilder b) => { };

        var provider = await CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<App1DbContext>(o =>
                {
                    o.Configure(appBuilderAction);
                    o.Entity<App1Entity1>(e => { });
                });

                builder.Services.AddAxiomDbContext<Module1DbContext>(o =>
                {
                    o.RegisterAsGenericDbContext = true;
                    o.Configure(moduleBuilderAction);
                    o.Entity<Module1Entity1>(e => { });
                });
            });

        var appOptions = provider.GetRequiredService<IOptions<AxiomDbContextOptions<App1DbContext>>>().Value;
        appOptions.Type.ShouldBe(typeof(App1DbContext));
        appOptions.BuilderAction.ShouldBe(appBuilderAction);
        appOptions.ConnectionStringName.ShouldBe("App1");
        appOptions.TenancySide.ShouldBe(TenancySide.Hybrid);
        appOptions.EntityOptions.ShouldContainKey(typeof(App1Entity1));

        var moduleOptions = provider.GetRequiredService<IOptions<AxiomDbContextOptions<Module1DbContext>>>().Value;
        moduleOptions.Type.ShouldBe(typeof(Module1DbContext));
        moduleOptions.BuilderAction.ShouldBe(moduleBuilderAction);
        moduleOptions.ConnectionStringName.ShouldBe("Module1");
        moduleOptions.TenancySide.ShouldBe(TenancySide.Host);
        moduleOptions.EntityOptions.ShouldContainKey(typeof(Module1Entity1));
    }

    [Fact]
    public async Task ShouldPreferConfiguredOptionsOverRegisteredOptions()
    {
        var appBuilderAction = (IServiceProvider sp, DbContextOptionsBuilder b) => { };

        var provider = await CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.ConfigureAxiomDbContext<App1DbContext>(o =>
                {
                    o.Configure(appBuilderAction);
                    o.Entity<App1Entity1>(e => { });
                });

                builder.Services.AddAxiomDbContext<App1DbContext>(o =>
                {
                    o.Configure(b =>
                    {
                        /* Trying change configuration */
                    });
                });
            });

        var appOptions = provider.GetRequiredService<IOptions<AxiomDbContextOptions<App1DbContext>>>().Value;
        appOptions.BuilderAction.ShouldBe(appBuilderAction);
        appOptions.EntityOptions.ShouldContainKey(typeof(App1Entity1));
    }

    // ConfigureConnectionStringOptions

    [Fact]
    public async Task ShouldMarkConnectionStringContextAsTenantAgnosticForHostDbContext()
    {
        var provider = await CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module1DbContext>(o => { o.RegisterAsGenericDbContext = true; });
            });

        var options = provider.GetRequiredService<IOptions<ConnectionStringContextsOptions>>().Value;
        var context = options.Contexts.Single(c => c.Name == "Module1");

        context.IsTenantAgnostic.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotMarkConnectionStringContextAsTenantAgnosticForTenantDbContext()
    {
        var provider = await CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module2DbContext>(o => { o.RegisterAsGenericDbContext = true; });
            });

        var options = provider.GetRequiredService<IOptions<ConnectionStringContextsOptions>>().Value;
        var context = options.Contexts.Single(c => c.Name == "Module2");

        context.IsTenantAgnostic.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldNotMarkConnectionStringContextAsTenantAgnosticForHybridDbContext()
    {
        var provider = await CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module3DbContext>(o => { o.RegisterAsGenericDbContext = true; });
            });

        var options = provider.GetRequiredService<IOptions<ConnectionStringContextsOptions>>().Value;
        var context = options.Contexts.Single(c => c.Name == "Module3");

        context.IsTenantAgnostic.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldConfigureOnlyReplacementConnectionStringContexts()
    {
        var provider = await CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<HybridDbContext>();

                builder.Services.AddAxiomDbContext<Module1DbContext>(o => { o.RegisterAsGenericDbContext = true; });
                builder.Services.AddAxiomDbContext<Module2DbContext>(o => { o.RegisterAsGenericDbContext = true; });
                builder.Services.AddAxiomDbContext<Module3DbContext>(o => { o.RegisterAsGenericDbContext = true; });
            });

        var options = provider.GetRequiredService<IOptions<ConnectionStringContextsOptions>>().Value;
        var dbContextOptions = provider.GetRequiredService<IOptions<AxiomDbContextOptions<HybridDbContext>>>().Value;

        options.Contexts.ShouldNotContain(c => c.Name == "Module1");
        options.Contexts.ShouldNotContain(c => c.Name == "Module2");
        options.Contexts.ShouldNotContain(c => c.Name == "Module3");

        var context = options.Contexts.Single(c => c.Name == dbContextOptions.ConnectionStringName);
        context.IsTenantAgnostic.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldConfigureOnlyReplacementConnectionStringContextsWithDifferentTenancySides()
    {
        var provider = await CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<HostSideDbContext>();
                builder.Services.AddAxiomDbContext<TenantSideDbContext>();

                builder.Services.AddAxiomDbContext<Module1DbContext>(o => { o.RegisterAsGenericDbContext = true; });
                builder.Services.AddAxiomDbContext<Module2DbContext>(o => { o.RegisterAsGenericDbContext = true; });
                builder.Services.AddAxiomDbContext<Module3DbContext>(o => { o.RegisterAsGenericDbContext = true; });
            });

        var options = provider.GetRequiredService<IOptions<ConnectionStringContextsOptions>>().Value;

        options.Contexts.ShouldNotContain(c => c.Name == "Module1");
        options.Contexts.ShouldNotContain(c => c.Name == "Module2");
        options.Contexts.ShouldNotContain(c => c.Name == "Module3");

        var hostOptions = provider.GetRequiredService<IOptions<AxiomDbContextOptions<HostSideDbContext>>>().Value;
        hostOptions.TenancySide.ShouldBe(TenancySide.Host);
        var hostContext = options.Contexts.Single(c => c.Name == hostOptions.ConnectionStringName);
        hostContext.IsTenantAgnostic.ShouldBeTrue();

        var tenantOptions = provider.GetRequiredService<IOptions<AxiomDbContextOptions<TenantSideDbContext>>>().Value;
        tenantOptions.TenancySide.ShouldBe(TenancySide.Tenant);
        var tenantContext = options.Contexts.Single(c => c.Name == tenantOptions.ConnectionStringName);
        tenantContext.IsTenantAgnostic.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldNotConfigureConnectionStringContextWhenAlreadyIncludedInAnotherContext()
    {
        var provider = await CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module1DbContext>(o => { o.RegisterAsGenericDbContext = true; });
                builder.Services.AddAxiomDbContext<Module2DbContext>(o => { o.RegisterAsGenericDbContext = true; });

                builder.Services.Configure<ConnectionStringContextsOptions>(o =>
                {
                    o.Contexts.Add(new ConnectionStringContextOptions
                    {
                        Name = "Grouped",
                        Connections = ["Module1", "Module2"],
                        IsTenantAgnostic = false
                    });
                });
            });

        var options = provider.GetRequiredService<IOptions<ConnectionStringContextsOptions>>().Value;
        options.Contexts.ShouldNotContain(c => c.Name == "Module1");
        options.Contexts.ShouldNotContain(c => c.Name == "Module2");

        var context = options.Contexts.Single(c => c.Name == "Grouped");
        context.IsTenantAgnostic.ShouldBeFalse();
    }
}

[ReplaceDbContext(typeof(Module1DbContext), typeof(Module2DbContext), typeof(Module3DbContext))]
file class HybridDbContext : DbContext
{
    public DbSet<Module1Entity1> Module1Entity1 { get; set; }
    public DbSet<Module1Entity2> Module1Entity2 { get; set; }
}

[TenancySide(TenancySide.Host)]
[ReplaceDbContext(typeof(Module1DbContext), typeof(Module2DbContext), typeof(Module3DbContext))]
file class HostSideDbContext : DbContext { }

[TenancySide(TenancySide.Tenant)]
[ReplaceDbContext(typeof(Module1DbContext), typeof(Module2DbContext), typeof(Module3DbContext))]
file class TenantSideDbContext : DbContext { }