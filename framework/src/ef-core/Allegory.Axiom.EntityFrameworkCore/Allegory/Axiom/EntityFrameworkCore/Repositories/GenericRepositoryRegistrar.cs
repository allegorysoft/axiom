using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

internal class GenericRepositoryRegistrar(
    Type dbContextType,
    AxiomDbContextOptionsBuilder builder,
    IServiceCollection services) :
    RepositoryRegistrarBase(dbContextType, builder, services)
{
    public override void Register()
    {
        Registrars[DbContextType] = this;

        // var rootDbContexts = Registrars.Values
        //     .Where(r => r.Builder.ReplacedDbContexts?.Contains(DbContextType) ?? false)
        //     .ToList();

        RegisterRepositories();
        RegisterDefaultRepositories();
    }

    protected void RegisterRepositories()
    {
        foreach (var repository in Builder.Repositories)
        {
            var repositoryImplementation = repository.Type.MakeGenericType(DbContextType);
            var descriptor = new RepositoryDescriptor(repositoryImplementation, Builder.ExposeGenericRepositories, repository.TenancySide);
            Descriptors.Add(descriptor);

            foreach (var serviceType in descriptor.Services)
            {
                Services.TryAdd(ServiceDescriptor.Describe(serviceType, repositoryImplementation,
                    Builder.ServiceLifetime));
            }
        }
    }

    protected void RegisterDefaultRepositories()
    {
        if (!Builder.RegisterDefaultRepositories)
        {
            return;
        }

        var entities = GetEntityTypes(DbContextType).Where(t => Descriptors.All(d => t != d.EntityType)).ToList();

        foreach (var descriptor in entities.Select(entityType => new RepositoryDescriptor(entityType, DbContextType)))
        {
            Descriptors.Add(descriptor);

            foreach (var serviceType in descriptor.Services)
            {
                Services.TryAdd(
                    ServiceDescriptor.Describe(serviceType, descriptor.ImplementationType, Builder.ServiceLifetime));
            }
        }
    }
}