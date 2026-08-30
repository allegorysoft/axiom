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
        SetTenancySide();
    }

    protected void RegisterRepositories()
    {
        foreach (var repository in Builder.Repositories)
        {
            var repositoryImplementation = repository.Type.MakeGenericType(Builder.DbContextType);

            var descriptor = new RepositoryDescriptor(
                repositoryImplementation,
                Builder.ExposeGenericServices,
                repository.TenancySide);

            Descriptors.Add(descriptor);

            foreach (var serviceType in descriptor.Services)
            {
                Services.TryAdd(
                    ServiceDescriptor.Describe(serviceType, repositoryImplementation, Builder.ServiceLifetime));
            }
        }
    }

    protected void RegisterDefaultRepositories()
    {
        if (!Builder.RegisterDefaultRepositories)
        {
            return;
        }

        var descriptors = GetEntityTypes(Builder.DbContextType)
            .Where(t => Descriptors.All(d => t != d.EntityType))
            .ToList()
            .Select(entityType => new RepositoryDescriptor(entityType, Builder.DbContextType));

        foreach (var descriptor in descriptors)
        {
            Descriptors.Add(descriptor);

            foreach (var serviceType in descriptor.Services)
            {
                Services.TryAdd(
                    ServiceDescriptor.Describe(serviceType, descriptor.ImplementationType, Builder.ServiceLifetime));
            }
        }
    }

    protected void SetTenancySide()
    {
        if (Descriptors.All(d => d.TenancySide == TenancySide.Host))
        {
            Builder.TenancySide = TenancySide.Host;
        }
        else if (Descriptors.All(d => d.TenancySide == TenancySide.Tenant))
        {
            Builder.TenancySide = TenancySide.Tenant;
        }
        else
        {
            Builder.TenancySide = TenancySide.Hybrid;
        }
    }

    public void ReplaceRepository(Type repository)
    {
        TenancySide? tenancySide = null;

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
                    tenancySide = oldDescriptor.TenancySide;
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
            Services.Replace(
                ServiceDescriptor.Describe(serviceType, repositoryImplementation, Builder.ServiceLifetime));
        }
    }
}