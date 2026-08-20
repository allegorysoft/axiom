using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

internal class GenericRepositoryRegistrar(
    AxiomDbContextOptionsBuilder builder,
    IServiceCollection services) :
    RepositoryRegistrarBase(builder, services)
{
    public override void Register()
    {
        RegisterRepositories();
        RegisterDefaultRepositories();
    }

    protected void RegisterRepositories()
    {
        foreach (var repository in Builder.Repositories)
        {
            var repositoryImplementation = repository.Type.MakeGenericType(Builder.DbContextType);

            var descriptor = new RepositoryDescriptor(
                repositoryImplementation,
                Builder.ExposeGenericRepositories,
                repository.TenancySide);

            Descriptors.Add(descriptor);

            foreach (var serviceType in descriptor.Services)
            {
                Services.TryAdd(ServiceDescriptor.Describe(serviceType, repositoryImplementation, Builder.ServiceLifetime));
            }
        }
    }

    protected void RegisterDefaultRepositories()
    {
        if (!Builder.RegisterDefaultRepositories)
        {
            return;
        }

        var entities = GetEntityTypes(Builder.DbContextType).Where(t => Descriptors.All(d => t != d.EntityType)).ToList();

        foreach (var descriptor in entities.Select(entityType => new RepositoryDescriptor(entityType, Builder.DbContextType)))
        {
            Descriptors.Add(descriptor);

            foreach (var serviceType in descriptor.Services)
            {
                Services.TryAdd(ServiceDescriptor.Describe(serviceType, descriptor.ImplementationType, Builder.ServiceLifetime));
            }
        }
    }
}