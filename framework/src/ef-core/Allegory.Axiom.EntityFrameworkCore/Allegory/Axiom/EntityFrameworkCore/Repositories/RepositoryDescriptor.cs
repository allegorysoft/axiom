using System;
using System.Linq;
using Allegory.Axiom.Domain.Entities;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

internal class RepositoryDescriptor
{
    public Type ServiceType { get; }
    public Type ImplementationType { get; set; } = null!;
    public Type? EntityType { get; }
    public Type? EntityKeyType { get; }
    public bool IsDefaultRepository { get; } // EfCoreRepository<TContext, TEntity, TKey?> or RepositoryImp<TContext>

    public RepositoryDescriptor(
        Type serviceType,
        bool isDefaultRepository, 
        Type? implementationType = null,
        Type? entityType = null)
    {
        ServiceType = serviceType;
        IsDefaultRepository = isDefaultRepository;
        EntityType = entityType;

        if (EntityType != null)
        {
            EntityKeyType = EntityType
                .GetInterfaces()
                .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntity<>))?
                .GetGenericArguments().Single() ?? null;
        }

        if (implementationType != null)
        {
            ImplementationType = implementationType;
        }
    }
}