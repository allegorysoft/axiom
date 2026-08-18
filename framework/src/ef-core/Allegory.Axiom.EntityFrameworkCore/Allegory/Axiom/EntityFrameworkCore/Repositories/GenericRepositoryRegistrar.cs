using System;
using System.Collections.Generic;
using System.Linq;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Repositories;
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

        RegisterRepositories();
        RegisterDefaultRepositories();
    }

    protected void RegisterRepositories()
    {
        foreach (var repository in Builder.Repositories)
        {
            var repositoryImplementation = repository.MakeGenericType(DbContextType);
            var serviceTypes = GetRepositoryServices(repositoryImplementation, out var entityType);

            foreach (var serviceType in serviceTypes)
            {
                Descriptors.Add(
                    new RepositoryDescriptor(
                        serviceType,
                        false,
                        implementationType: repository,
                        entityType: entityType));

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

        var existingRepos = Descriptors.Where(d => d.EntityType != null).Select(d => d.EntityType!).ToHashSet();
        var entities = GetEntityTypes(DbContextType).Where(t => !existingRepos.Contains(t)).ToList();

        foreach (var entityType in entities)
        {
            RegisterDefaultRepository(entityType);
        }
    }

    protected void RegisterDefaultRepository(Type entityType)
    {
        var keyedEntity = entityType
            .GetInterfaces()
            .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntity<>));

        Type repositoryType;
        var serviceTypes = new List<Type>(2);

        if (keyedEntity == null)
        {
            repositoryType = typeof(EfCoreRepository<,>).MakeGenericType(DbContextType, entityType);
            serviceTypes.Add(typeof(IReadOnlyRepository<>).MakeGenericType(entityType));
            serviceTypes.Add(typeof(IRepository<>).MakeGenericType(entityType));
        }
        else
        {
            var keyType = keyedEntity.GetGenericArguments().Single();
            repositoryType =
                typeof(EfCoreRepository<,,>).MakeGenericType(DbContextType, entityType, keyType);
            serviceTypes.Add(typeof(IReadOnlyRepository<,>).MakeGenericType(entityType, keyType));
            serviceTypes.Add(typeof(IRepository<,>).MakeGenericType(entityType, keyType));
        }

        foreach (var serviceType in serviceTypes)
        {
            Descriptors.Add(
                new RepositoryDescriptor(
                    serviceType,
                    true,
                    implementationType: repositoryType,
                    entityType: entityType));

            Services.TryAdd(ServiceDescriptor.Describe(serviceType, repositoryType, Builder.ServiceLifetime));
        }
    }
}