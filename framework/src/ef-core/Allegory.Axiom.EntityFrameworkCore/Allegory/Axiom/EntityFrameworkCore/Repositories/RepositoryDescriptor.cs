using System;
using System.Collections.Generic;
using System.Linq;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.MultiTenancy;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

internal class RepositoryDescriptor
{
    public Type ImplementationType { get; set; }
    public bool IsDefaultRepository { get; }
    public Type? EntityType { get; }
    public Type? EntityKeyType { get; }
    public TenancySide TenancySide { get; }
    public IReadOnlySet<Type> Services { get; }

    public RepositoryDescriptor(Type implementationType, bool exposeGenericServices, TenancySide? tenancySide = null)
    {
        ImplementationType = implementationType;
        IsDefaultRepository = false;
        Services = GetRepositoryServices(implementationType, exposeGenericServices, out var entityType);
        EntityType = entityType;

        if (EntityType == null)
        {
            TenancySide = tenancySide ?? throw new ArgumentNullException(
                nameof(tenancySide),
                $"During '{ImplementationType}' registration: {nameof(tenancySide)} must be provided " +
                $"because {nameof(EntityType)} is null. At least one of the two is required to resolve tenancy.");
        }
        else
        {
            EntityKeyType = EntityType
                .GetInterfaces()
                .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntity<>))?
                .GetGenericArguments().Single() ?? null;

            TenancySide = typeof(ITenantOwned).IsAssignableFrom(EntityType) ? TenancySide.Tenant : TenancySide.Host;
        }
    }

    public RepositoryDescriptor(Type entityType, Type dbContextType)
    {
        IsDefaultRepository = true;
        EntityType = entityType;
        TenancySide = typeof(ITenantOwned).IsAssignableFrom(EntityType) ? TenancySide.Tenant : TenancySide.Host;

        var keyedEntity = entityType
            .GetInterfaces()
            .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntity<>));

        var services = new HashSet<Type>(2);

        if (keyedEntity == null)
        {
            ImplementationType = typeof(EfCoreRepository<,>).MakeGenericType(dbContextType, entityType);
            services.Add(typeof(IReadOnlyRepository<>).MakeGenericType(entityType));
            services.Add(typeof(IRepository<>).MakeGenericType(entityType));
        }
        else
        {
            EntityKeyType = keyedEntity.GetGenericArguments().Single();
            ImplementationType = typeof(EfCoreRepository<,,>).MakeGenericType(dbContextType, entityType, EntityKeyType);
            services.Add(typeof(IReadOnlyRepository<,>).MakeGenericType(entityType, EntityKeyType));
            services.Add(typeof(IRepository<,>).MakeGenericType(entityType, EntityKeyType));
        }

        Services = services;
    }

    private static HashSet<Type> GetRepositoryServices(Type type, bool exposeGenericRepositories, out Type? entityType)
    {
        var interfaces = type.GetInterfaces();
        var list = new HashSet<Type>();
        entityType = null;

        // IEntityRepository; IProductRepository, IOrderRepository etc.
        RegisterRepositoryService(interfaces, type, list);

        // Entity type for IReadOnlyRepository<TEntity>; null when the repository has no explicit entity type
        var readOnlyRepository = TryGetReadOnlyGenericRepository(interfaces, out entityType);

        if (readOnlyRepository == null || !exposeGenericRepositories)
        {
            if (list.Count == 0)
            {
                throw new InvalidOperationException(
                    $"'{type}' does not implement any interface, so it cannot be registered. " +
                    $"Custom repository implementations must define and implement their own interface " +
                    $"(e.g. 'I{type.Name}') so they can be resolved through dependency injection.");
            }

            return list;
        }

        list.Add(readOnlyRepository); // IReadOnlyRepository<TEntity>
        AddIfMatch(interfaces, typeof(IRepository<>), list); // IRepository<TEntity>
        AddIfMatch(interfaces, typeof(IReadOnlyRepository<,>), list); // IReadOnlyRepository<TEntity, TKey>
        AddIfMatch(interfaces, typeof(IRepository<,>), list); // IRepository<TEntity, TKey>

        return list;

        static void RegisterRepositoryService(IEnumerable<Type> interfaces, Type type, HashSet<Type> list)
        {
            var nonGenericRepository = interfaces.SingleOrDefault(r =>
                typeof(IRepository).IsAssignableFrom(r) && !r.IsGenericType && r != typeof(IRepository));

            if (nonGenericRepository == null)
            {
                // If repository implementation not generic (AppDbContext)
                // Register implementation type as service type
                if (!type.IsGenericType)
                {
                    list.Add(type);
                }
            }
            else
            {
                list.Add(nonGenericRepository);
            }
        }

        static Type? TryGetReadOnlyGenericRepository(IEnumerable<Type> interfaces, out Type? entityType)
        {
            var repository = interfaces.SingleOrDefault(x =>
                x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IReadOnlyRepository<>));
            entityType = repository?.GenericTypeArguments[0];

            return repository;
        }
        
        static void AddIfMatch(IEnumerable<Type> interfaces, Type genericDefinition, HashSet<Type> list)
        {
            var match = interfaces.SingleOrDefault(x =>
                x.IsGenericType && x.GetGenericTypeDefinition() == genericDefinition);
            if (match != null)
                list.Add(match);
        }
    }
}