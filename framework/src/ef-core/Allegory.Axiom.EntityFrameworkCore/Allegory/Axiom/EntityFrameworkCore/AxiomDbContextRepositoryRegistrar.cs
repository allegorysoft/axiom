using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Allegory.Axiom.EntityFrameworkCore;

public class AxiomDbContextRepositoryRegistrar(
    Type type,
    AxiomDbContextOptionsBuilder builder,
    IServiceCollection services)
{
    public Type Type { get; } = type;
    public AxiomDbContextOptionsBuilder Builder { get; } = builder;
    public IServiceCollection Services { get; } = services;

    public void Register()
    {
        if (Builder.Repositories.Count == 0)
        {
            RegisterNonGenericRepositories();
        }
        else
        {
            RegisterRepositories();
        }
    }

    protected void RegisterNonGenericRepositories()
    {
        var repositories = Type.Assembly
            .GetTypes()
            .Where(t => typeof(IRepository).IsAssignableFrom(t)
                        && !t.IsGenericType && t is {IsClass: true, IsAbstract: false})
            .ToHashSet();

        var registeredEntities = new HashSet<Type>();
        foreach (var repository in repositories)
        {
            var serviceTypes = GetRepositoryServices(repository, out var entityType);

            if (entityType != null)
            {
                registeredEntities.Add(entityType);
            }

            foreach (var serviceType in serviceTypes)
            {
                Services.TryAdd(ServiceDescriptor.Describe(serviceType, repository, Builder.ServiceLifetime));
            }
        }

        TryRegisterDefaultRepositoriesForNonGeneric(registeredEntities);
    }

    protected void TryRegisterDefaultRepositoriesForNonGeneric(HashSet<Type> registered)
    {
        if (!Builder.RegisterDefaultRepositories)
        {
            return;
        }

        var entities = GetEntityTypes(Type).Where(t => !registered.Contains(t)).ToList();

        foreach (var entityType in entities)
        {
            var keyedEntity = entityType
                .GetInterfaces()
                .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntity<>));

            if (keyedEntity == null)
            {
                var implementationType = typeof(EfCoreRepository<,>).MakeGenericType(Type, entityType);
                Services.TryAdd(ServiceDescriptor.Describe(
                    typeof(IReadOnlyRepository<>).MakeGenericType(entityType),
                    implementationType,
                    Builder.ServiceLifetime));
                Services.TryAdd(ServiceDescriptor.Describe(
                    typeof(IRepository<>).MakeGenericType(entityType),
                    implementationType,
                    Builder.ServiceLifetime));
            }
            else
            {
                var keyType = keyedEntity.GetGenericArguments().Single();
                var implementationType = typeof(EfCoreRepository<,,>).MakeGenericType(Type, entityType, keyType);
                Services.TryAdd(ServiceDescriptor.Describe(
                    typeof(IReadOnlyRepository<,>).MakeGenericType(entityType, keyType),
                    implementationType,
                    Builder.ServiceLifetime));
                Services.TryAdd(ServiceDescriptor.Describe(
                    typeof(IRepository<,>).MakeGenericType(entityType, keyType),
                    implementationType,
                    Builder.ServiceLifetime));
            }
        }
    }

    protected void RegisterRepositories()
    {
        foreach (var repository in Builder.Repositories)
        {
            var implementationType = repository.IsGenericType
                ? repository.MakeGenericType(Type)
                : repository;

            var serviceTypes = GetRepositoryServices(implementationType, out var entityType);

            foreach (var serviceType in serviceTypes)
            {
                Services.TryAdd(ServiceDescriptor.Describe(serviceType, implementationType, Builder.ServiceLifetime));
            }
        }
    }

    protected List<Type> GetRepositoryServices(Type type, out Type? entityType)
    {
        var interfaces = type.GetInterfaces();
        var list = new List<Type>();
        entityType = null;

        // IEntityRepository; IProductRepository, IOrderRepository etc.
        var nonGenericRepository = interfaces.SingleOrDefault(r =>
            typeof(IRepository).IsAssignableFrom(r) && !r.IsGenericType && r != typeof(IRepository));
        if (nonGenericRepository != null)
            list.Add(nonGenericRepository);

        // Entity type for IReadOnlyRepository<TEntity>; null when the repository has no explicit entity type
        var readOnlyRepository = interfaces.SingleOrDefault(x =>
            x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IReadOnlyRepository<>));
        entityType = readOnlyRepository?.GenericTypeArguments[0];

        if (readOnlyRepository == null || !Builder.ExposeGenericRepositories)
        {
            return list;
        }

        list.Add(readOnlyRepository); // IReadOnlyRepository<TEntity>
        AddIfMatch(interfaces, typeof(IRepository<>), list); // IRepository<TEntity>
        AddIfMatch(interfaces, typeof(IReadOnlyRepository<,>), list); // IReadOnlyRepository<TEntity, TKey>
        AddIfMatch(interfaces, typeof(IRepository<,>), list); // IRepository<TEntity, TKey>

        return list;

        static void AddIfMatch(IEnumerable<Type> interfaces, Type genericDefinition, List<Type> list)
        {
            var match = interfaces.SingleOrDefault(x =>
                x.IsGenericType && x.GetGenericTypeDefinition() == genericDefinition);
            if (match != null)
                list.Add(match);
        }
    }

    protected static IReadOnlyList<Type> GetEntityTypes(Type type)
    {
        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToList();
    }
}