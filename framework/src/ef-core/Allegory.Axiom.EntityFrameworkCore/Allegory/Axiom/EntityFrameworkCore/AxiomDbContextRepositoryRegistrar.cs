using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        RegisterNonGenericRepositories();

        foreach (var repository in Builder.Repositories)
        {
            var implementationType = repository.IsGenericType
                ? repository.MakeGenericType(Type)
                : repository;

            var serviceTypes = GetRepositoryServices(implementationType, out var entityType);

            foreach (var serviceType in serviceTypes)
            {
                Services.AddSingleton(serviceType, implementationType);
            }
        }
    }

    protected void RegisterNonGenericRepositories()
    {
        var repositories = Type.Assembly
            .GetTypes()
            .Where(t => typeof(IRepository).IsAssignableFrom(t)
                        && !t.IsGenericType && t is {IsClass: true, IsAbstract: false})
            .ToHashSet();

        foreach (var repository in repositories)
        {
            var serviceTypes = GetRepositoryServices(repository, out _);

            foreach (var serviceType in serviceTypes)
            {
                Services.TryAdd(ServiceDescriptor.Describe(serviceType, repository, Builder.ServiceLifetime));
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

    private static IReadOnlyList<Type> GetEntityTypes(Type type)
    {
        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToList();
    }
}