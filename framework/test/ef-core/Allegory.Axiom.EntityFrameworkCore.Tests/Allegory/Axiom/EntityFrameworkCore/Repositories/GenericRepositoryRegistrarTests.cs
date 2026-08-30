using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.EntityFrameworkCore.DbContexts;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

public class GenericRepositoryRegistrarTests : IntegrationTest
{
    public override ValueTask InitializeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ShouldRegisterRepositories()
    {
        await CreateServiceProviderAsync(
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
    public async Task ShouldSetCorrectTenancySideAndConnectionString()
    {
        var provider = await CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module1DbContext>(o => { o.RegisterAsGenericDbContext = true; });
                builder.Services.AddAxiomDbContext<Module2DbContext>(o => { o.RegisterAsGenericDbContext = true; });
                builder.Services.AddAxiomDbContext<Module3DbContext>(o => { o.RegisterAsGenericDbContext = true; });
            },
            postConfigure: builder =>
            {
                var properties = builder.Services.GetExtraProperties();

                var module1 = properties.GenericRegistrars[typeof(Module1DbContext)].Builder;
                module1.TenancySide.ShouldBe(TenancySide.Host);
                module1.ConnectionStringName.ShouldBe("Module1");

                var module2 = properties.GenericRegistrars[typeof(Module2DbContext)].Builder;
                module2.TenancySide.ShouldBe(TenancySide.Tenant);
                module2.ConnectionStringName.ShouldBe("Module2");

                var module3 = properties.GenericRegistrars[typeof(Module3DbContext)].Builder;
                module3.TenancySide.ShouldBe(TenancySide.Hybrid);
                module3.ConnectionStringName.ShouldBe("Module3");
            });

        var module1Options = provider.GetRequiredService<IOptions<AxiomDbContextOptions<Module1DbContext>>>().Value;
        module1Options.TenancySide.ShouldBe(TenancySide.Host);
        module1Options.ConnectionStringName.ShouldBe("Module1");

        var module2Options = provider.GetRequiredService<IOptions<AxiomDbContextOptions<Module2DbContext>>>().Value;
        module2Options.TenancySide.ShouldBe(TenancySide.Tenant);
        module2Options.ConnectionStringName.ShouldBe("Module2");
        
        var module3Options = provider.GetRequiredService<IOptions<AxiomDbContextOptions<Module3DbContext>>>().Value;
        module3Options.TenancySide.ShouldBe(TenancySide.Hybrid);
        module3Options.ConnectionStringName.ShouldBe("Module3");
    }

    [Fact]
    public async Task ShouldUseConfiguredServiceLifetime()
    {
        await CreateServiceProviderAsync(
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
        await CreateServiceProviderAsync(
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
        await CreateServiceProviderAsync(
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
    public async Task ShouldExposeGenericServicesForCoveredEntityWhenEnabled()
    {
        await CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module1DbContext>(o =>
                {
                    o.ExposeGenericServices = true;
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
    public async Task ShouldNotExposeGenericServicesForCoveredEntityWhenDisabled()
    {
        await CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module1DbContext>(o =>
                {
                    o.ExposeGenericServices = false;
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

    [Fact]
    public async Task ShouldReplaceExistingRepositoryWithCustomRepository()
    {
        await CreateServiceProviderAsync(
            configure: builder =>
            {
                builder.Services.AddAxiomDbContext<Module1DbContext>(o =>
                {
                    o.ExposeGenericServices = true;
                    o.AddRepository(typeof(EfCoreModule1Entity1Repository<>));
                });

                builder.Services.ReplaceRepository<Module1DbContext>(typeof(CustomEfCoreModule1Entity1Repository<>));
            },
            postConfigure: builder =>
            {
                var descriptor1 =
                    builder.Services.Single(d => d.ServiceType == typeof(IModule1Entity1Repository));
                descriptor1.ImplementationType.ShouldBe(typeof(CustomEfCoreModule1Entity1Repository<Module1DbContext>));

                var descriptor2 =
                    builder.Services.Single(d => d.ServiceType == typeof(IRepository<Module1Entity1, int>));
                descriptor2.ImplementationType.ShouldBe(typeof(CustomEfCoreModule1Entity1Repository<Module1DbContext>));
            });
    }
}

file class CustomEfCoreModule1Entity1Repository<TDbContext>(
    IDbContextProvider<TDbContext> dbContextProvider)
    : EfCoreModule1Entity1Repository<TDbContext>(dbContextProvider)
    where TDbContext : DbContext { }