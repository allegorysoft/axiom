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

            Repositories.Add(repositoryImplementation);

            var serviceTypes = GetRepositoryServices(repositoryImplementation, out var entityType);

            if (entityType != null)
            {
                EntityRepositoryMap[entityType] = repositoryImplementation;
            }

            foreach (var serviceType in serviceTypes)
            {
                ServiceRepositoryMap[serviceType] = repositoryImplementation;

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

        var entities = GetEntityTypes(DbContextType).Where(t => !EntityRepositoryMap.ContainsKey(t)).ToList();

        foreach (var entityType in entities)
        {
            RegisterRepository(entityType);
        }
    }

    protected void RegisterRepository(Type entityType)
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

        Repositories.Add(repositoryType);
        EntityRepositoryMap[entityType] = repositoryType;

        foreach (var serviceType in serviceTypes)
        {
            ServiceRepositoryMap[serviceType] = repositoryType;
            Services.TryAdd(ServiceDescriptor.Describe(serviceType, repositoryType, Builder.ServiceLifetime));
        }
    }
}