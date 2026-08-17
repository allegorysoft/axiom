using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Allegory.Axiom.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

internal abstract class RepositoryRegistrarBase(
    Type dbContextType,
    AxiomDbContextOptionsBuilder builder,
    IServiceCollection services)
{
    protected Type DbContextType { get; } = dbContextType;
    protected AxiomDbContextOptionsBuilder Builder { get; } = builder;
    protected IServiceCollection Services { get; } = services;
    protected HashSet<Type> Repositories { get; } = new(); // ImplementationType
    protected Dictionary<Type, Type> EntityRepositoryMap { get; } = new(); // EntityType, ImplementationType
    protected Dictionary<Type, Type> ServiceRepositoryMap { get; } = new(); // ServiceType, ImplementationType

    public abstract void Register();

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

    public static RepositoryRegistrarBase Create(
        Type dbContextType,
        AxiomDbContextOptionsBuilder builder,
        IServiceCollection services)
    {
        return builder.Repositories.Count > 0
            ? new GenericRepositoryRegistrar(dbContextType, builder, services)
            : new RepositoryRegistrar(dbContextType, builder, services);
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