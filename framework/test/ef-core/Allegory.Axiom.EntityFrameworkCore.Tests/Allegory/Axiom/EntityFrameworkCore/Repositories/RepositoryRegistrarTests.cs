using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.EntityFrameworkCore.DbContexts;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

public class RepositoryRegistrarTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task ShouldRegisterRepositories()
    {
        await fixture.CreateServiceProviderAsync(
            configure: builder => { builder.Services.AddAxiomDbContext<App1DbContext>(); },
            postConfigure: builder =>
            {
                var descriptor = builder.Services.Single(d => d.ServiceType == typeof(IApp1Entity1Repository));
                descriptor.ImplementationType.ShouldBe(typeof(EfCoreApp1Entity1Repository));

                var descriptor2 = builder.Services.Single(d => d.ServiceType == typeof(IRepository<App1Entity2, int>));
                descriptor2.ImplementationType.ShouldBe(typeof(EfCoreRepository<App1DbContext, App1Entity2, int>));
            });
    }

    [Fact]
    public async Task ShouldUseConfiguredServiceLifetime()
    {
        await fixture.CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<App1DbContext>(o =>
                {
                    o.ServiceLifetime = ServiceLifetime.Transient;
                });
            },
            postConfigure: builder =>
            {
                var descriptor = builder.Services.Single(d => d.ServiceType == typeof(IApp1Entity1Repository));
                descriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
            });
    }

    [Fact]
    public async Task ShouldRegisterDefaultRepositoryForUncoveredEntityWhenEnabled()
    {
        await fixture.CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<App1DbContext>(o => { o.RegisterDefaultRepositories = true; });
            },
            postConfigure: builder =>
            {
                var descriptor =
                    builder.Services.Single(d => d.ServiceType == typeof(IRepository<App1Entity2, int>));
                descriptor.ImplementationType.ShouldBe(typeof(EfCoreRepository<App1DbContext, App1Entity2, int>));
            });
    }

    [Fact]
    public async Task ShouldNotRegisterDefaultRepositoryForUncoveredEntityWhenDisabled()
    {
        await fixture.CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<App1DbContext>(o => { o.RegisterDefaultRepositories = false; });
            },
            postConfigure: builder =>
            {
                builder.Services.ShouldNotContain(d => d.ServiceType == typeof(IRepository<App1Entity2, int>));
            });
    }

    [Fact]
    public async Task ShouldExposeGenericRepositoryForCoveredEntityWhenEnabled()
    {
        await fixture.CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<App1DbContext>(o => { o.ExposeGenericRepositories = true; });
            },
            postConfigure: builder =>
            {
                var descriptor1 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IRepository<App1Entity1, int>));
                descriptor1.ShouldNotBeNull();
                descriptor1.ImplementationType.ShouldBe(typeof(EfCoreApp1Entity1Repository));

                var descriptor2 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IApp1Entity1Repository));
                descriptor2.ShouldNotBeNull();
                descriptor2.ImplementationType.ShouldBe(typeof(EfCoreApp1Entity1Repository));
            });
    }

    [Fact]
    public async Task ShouldNotExposeGenericRepositoryForCoveredEntityWhenDisabled()
    {
        await fixture.CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<App1DbContext>(o => { o.ExposeGenericRepositories = false; });
            },
            postConfigure: builder =>
            {
                var descriptor1 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IRepository<App1Entity1, int>));
                descriptor1.ShouldBeNull();

                var descriptor2 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IApp1Entity1Repository));
                descriptor2.ShouldNotBeNull();
                descriptor2.ImplementationType.ShouldBe(typeof(EfCoreApp1Entity1Repository));
            });
    }

    [Fact]
    public async Task ShouldUseSpecifiedDbContextForReplacedDbContexts()
    {
        await fixture.CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module1DbContext>(o =>
                {
                    o.AddRepository(typeof(EfCoreModule1Entity1Repository<>));
                });
                builder.Services.AddAxiomDbContext<Module2DbContext>(o => { o.RegisterAsGenericDbContext = true; });
                builder.Services.AddAxiomDbContext<Module3DbContext>(o => { o.RegisterAsGenericDbContext = true; });

                builder.Services.AddAxiomDbContext<App1DbContext>();
                builder.Services.AddAxiomDbContext<HybridDbContext>();
            },
            postConfigure: builder =>
            {
                var descriptor1 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IModule1Entity1Repository));
                descriptor1.ShouldNotBeNull();
                descriptor1.ImplementationType.ShouldBe(typeof(EfCoreModule1Entity1Repository<HybridDbContext>));

                var descriptor2 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IRepository<Module2Entity1, int>));
                descriptor2.ShouldNotBeNull();
                descriptor2.ImplementationType.ShouldBe(
                    typeof(EfCoreRepository<HybridDbContext, Module2Entity1, int>));

                var descriptor3 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IRepository<Module3Entity1, int>));
                descriptor3.ShouldNotBeNull();
                descriptor3.ImplementationType.ShouldBe(
                    typeof(EfCoreRepository<HybridDbContext, Module3Entity1, int>));
            });
    }

    [Fact]
    public async Task ShouldRespectTenancySideWhenReplacingDbContext()
    {
        await fixture.CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module1DbContext>(o =>
                {
                    o.AddRepository(typeof(EfCoreModule1ReportRepository<>), TenancySide.Host);
                });
                builder.Services.AddAxiomDbContext<Module2DbContext>(o => { o.RegisterAsGenericDbContext = true; });
                builder.Services.AddAxiomDbContext<Module3DbContext>(o => { o.RegisterAsGenericDbContext = true; });

                builder.Services.AddAxiomDbContext<App1DbContext>();
                builder.Services.AddAxiomDbContext<HostSideDbContext>();
                builder.Services.AddAxiomDbContext<TenantSideDbContext>();
            },
            postConfigure: builder =>
            {
                var descriptor1 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IModule1ReportRepository));
                descriptor1.ShouldNotBeNull();
                descriptor1.ImplementationType.ShouldBe(typeof(EfCoreModule1ReportRepository<HostSideDbContext>));

                var descriptor2 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IRepository<Module2Entity1, int>));
                descriptor2.ShouldNotBeNull();
                descriptor2.ImplementationType.ShouldBe(
                    typeof(EfCoreRepository<TenantSideDbContext, Module2Entity1, int>));

                var descriptor3 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IRepository<Module3Entity1, int>));
                descriptor3.ShouldNotBeNull();
                descriptor3.ImplementationType.ShouldBe(
                    typeof(EfCoreRepository<HostSideDbContext, Module3Entity1, int>));

                var descriptor4 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IRepository<Module3Entity2, int>));
                descriptor4.ShouldNotBeNull();
                descriptor4.ImplementationType.ShouldBe(
                    typeof(EfCoreRepository<TenantSideDbContext, Module3Entity2, int>));
            });
    }
}

file class App1DbContext : DbContext
{
    public DbSet<App1Entity1> Entity1 { get; set; }
    public DbSet<App1Entity2> Entity2 { get; set; }
}

file class App1Entity1 : AggregateRoot<int> { }

file class App1Entity2 : AggregateRoot<int> { }

file interface IApp1Entity1Repository : IRepository<App1Entity1, int> { }

file class EfCoreApp1Entity1Repository(
    IDbContextProvider<App1DbContext> dbContextProvider)
    : EfCoreRepository<App1DbContext, App1Entity1, int>(dbContextProvider), IApp1Entity1Repository { }

[ReplaceDbContext(typeof(Module1DbContext), typeof(Module2DbContext), typeof(Module3DbContext))]
file class HybridDbContext : DbContext { }

[TenancySide(TenancySide.Host)]
[ReplaceDbContext(typeof(Module1DbContext), typeof(Module2DbContext), typeof(Module3DbContext))]
file class HostSideDbContext : DbContext { }

[TenancySide(TenancySide.Tenant)]
[ReplaceDbContext(typeof(Module1DbContext), typeof(Module2DbContext), typeof(Module3DbContext))]
file class TenantSideDbContext : DbContext
{
    public DbSet<Module2Entity1> Module2Entity1 { get; set; }
    public DbSet<Module2Entity2> Module2Entity2 { get; set; }

    public DbSet<Module3Entity2> Module3Entity2 { get; set; }
}