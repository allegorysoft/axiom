using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.EntityFrameworkCore.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

public class GenericRepositoryRegistrarTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task ShouldRegisterRepositories()
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
            },
            postConfigure: builder =>
            {
                var descriptor1 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IModule1Entity1Repository));
                descriptor1.ShouldNotBeNull();
                descriptor1.ImplementationType.ShouldBe(typeof(EfCoreModule1Entity1Repository<Module1DbContext>));

                var descriptor2 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IRepository<Module2Entity1, int>));
                descriptor2.ShouldNotBeNull();
                descriptor2.ImplementationType.ShouldBe(
                    typeof(EfCoreRepository<Module2DbContext, Module2Entity1, int>));

                var descriptor3 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IRepository<Module3Entity1, int>));
                descriptor3.ShouldNotBeNull();
                descriptor3.ImplementationType.ShouldBe(
                    typeof(EfCoreRepository<Module3DbContext, Module3Entity1, int>));
            });
    }

    [Fact]
    public async Task ShouldUseConfiguredServiceLifetime()
    {
        await fixture.CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module2DbContext>(o =>
                {
                    o.ServiceLifetime = ServiceLifetime.Transient;
                    o.RegisterAsGenericDbContext = true;
                });

                builder.Services.AddAxiomDbContext<Module3DbContext>(o => { o.RegisterAsGenericDbContext = true; });
            },
            postConfigure: b =>
            {
                var descriptor1 = b.Services.Single(d => d.ServiceType == typeof(IRepository<Module2Entity1, int>));
                descriptor1.Lifetime.ShouldBe(ServiceLifetime.Transient);

                var descriptor2 = b.Services.Single(d => d.ServiceType == typeof(IRepository<Module3Entity1, int>));
                descriptor2.Lifetime.ShouldBe(ServiceLifetime.Singleton);
            });
    }

    [Fact]
    public async Task ShouldRegisterDefaultRepositoryForUncoveredEntityWhenEnabled()
    {
        await fixture.CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module2DbContext>(o =>
                {
                    o.RegisterAsGenericDbContext = true;
                    o.RegisterDefaultRepositories = true;
                });
            },
            postConfigure: builder =>
            {
                var descriptor =
                    builder.Services.Single(d => d.ServiceType == typeof(IRepository<Module2Entity1, int>));
                descriptor.ImplementationType.ShouldBe(typeof(EfCoreRepository<Module2DbContext, Module2Entity1, int>));
            });
    }

    [Fact]
    public async Task ShouldNotRegisterDefaultRepositoryForUncoveredEntityWhenDisabled()
    {
        await fixture.CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module2DbContext>(o =>
                {
                    o.RegisterAsGenericDbContext = true;
                    o.RegisterDefaultRepositories = false;
                });
            },
            postConfigure: builder =>
            {
                builder.Services.ShouldNotContain(d => d.ServiceType == typeof(IRepository<Module2Entity1, int>));
            });
    }

    [Fact]
    public async Task ShouldExposeGenericRepositoryForCoveredEntityWhenEnabled()
    {
        await fixture.CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module1DbContext>(o =>
                {
                    o.ExposeGenericRepositories = true;
                    o.AddRepository(typeof(EfCoreModule1Entity1Repository<>));
                });
            },
            postConfigure: builder =>
            {
                var descriptor1 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IRepository<Module1Entity1, int>));
                descriptor1.ShouldNotBeNull();
                descriptor1.ImplementationType.ShouldBe(typeof(EfCoreModule1Entity1Repository<Module1DbContext>));

                var descriptor2 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IModule1Entity1Repository));
                descriptor2.ShouldNotBeNull();
                descriptor2.ImplementationType.ShouldBe(typeof(EfCoreModule1Entity1Repository<Module1DbContext>));
            });
    }

    [Fact]
    public async Task ShouldNotExposeGenericRepositoryForCoveredEntityWhenDisabled()
    {
        await fixture.CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module1DbContext>(o =>
                {
                    o.ExposeGenericRepositories = false;
                    o.AddRepository(typeof(EfCoreModule1Entity1Repository<>));
                });
            },
            postConfigure: builder =>
            {
                var descriptor1 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IRepository<Module1Entity1, int>));
                descriptor1.ShouldBeNull();

                var descriptor2 =
                    builder.Services.SingleOrDefault(d => d.ServiceType == typeof(IModule1Entity1Repository));
                descriptor2.ShouldNotBeNull();
                descriptor2.ImplementationType.ShouldBe(typeof(EfCoreModule1Entity1Repository<Module1DbContext>));
            });
    }
}