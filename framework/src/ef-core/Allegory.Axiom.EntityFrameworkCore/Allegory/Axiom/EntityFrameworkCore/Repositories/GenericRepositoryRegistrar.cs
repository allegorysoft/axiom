using System;
using System.Linq;
using Allegory.Axiom.MultiTenancy;
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
                Builder.ExposeGenericServices,
                repository.TenancySide ?? Builder.TenancySide);

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

    public void ReplaceRepository(Type repository, TenancySide? tenancySide = null)
    {
        foreach (var existingRepository in Builder.Repositories)
        {
            var type = repository;

            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == existingRepository.Type)
                {
                    Builder.Repositories.Remove(existingRepository);
                    var oldRepositoryType = existingRepository.Type.MakeGenericType(Builder.DbContextType);
                    var oldDescriptor = Descriptors.Single(d => d.ImplementationType == oldRepositoryType);
                    Descriptors.Remove(oldDescriptor);
                    break;
                }

                type = type.BaseType;
            }
        }

        Builder.AddRepository(repository, tenancySide);

        var repositoryImplementation = repository.MakeGenericType(Builder.DbContextType);
        var descriptor = new RepositoryDescriptor(
            repositoryImplementation,
            Builder.ExposeGenericServices,
            tenancySide);
        Descriptors.Add(descriptor);

        foreach (var serviceType in descriptor.Services)
        {
            Services.Replace(ServiceDescriptor.Describe(serviceType, repositoryImplementation, Builder.ServiceLifetime));
        }
    }
}