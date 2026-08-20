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