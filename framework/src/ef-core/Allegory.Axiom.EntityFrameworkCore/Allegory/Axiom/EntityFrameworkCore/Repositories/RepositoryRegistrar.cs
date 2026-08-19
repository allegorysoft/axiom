using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

internal class RepositoryRegistrar(
    Type dbContextType,
    AxiomDbContextOptionsBuilder builder,
    IServiceCollection services) :
    RepositoryRegistrarBase(dbContextType, builder, services)
{
    protected IReadOnlySet<GenericRepositoryRegistrar> ReplacedRegistrars { get; set; } = null!;

    public override void Register()
    {
        ReplacedRegistrars = Builder.ReplacedDbContexts == null
            ? ImmutableHashSet<GenericRepositoryRegistrar>.Empty
            : Builder.ReplacedDbContexts.Select(x => GenericRegistrars[x]).ToHashSet();

        // Registers concrete (closed) repository implementations;
        // DbContext generic parameter is already fixed to a specific context type, such as
        // ProductRepository : EfCoreRepository<AppDbContext, Entity, Key>
        RegisterRepositories();

        // Discovers every DbSet<TEntity> exposed on the DbContext and, for any entity that doesn't
        // already have a repository registered, closes EfCoreRepository<TDbContext, TEntity, TKey>
        // via MakeGenericType and registers it as both IReadOnlyRepository<TEntity> and IRepository<TEntity>
        RegisterDefaultRepositories();

        // Replaces already-registered repository implementations (e.g. for entities also mapped
        // in another DbContext) with ones bound to this registrar's DbContext, so later
        // registrations for those entities resolve against this context instead.
        ReplaceRepositories();
    }

    protected void RegisterRepositories()
    {
        var repositories = DbContextType.Assembly
            .GetTypes()
            .Where(t => typeof(IRepository).IsAssignableFrom(t)
                        && !t.IsGenericType && t is {IsClass: true, IsAbstract: false})
            .ToHashSet();

        foreach (var repository in repositories)
        {
            var descriptor = new RepositoryDescriptor(repository, Builder.ExposeGenericRepositories);
            Descriptors.Add(descriptor);

            foreach (var serviceType in descriptor.Services)
            {
                Services.TryAdd(ServiceDescriptor.Describe(serviceType, repository, Builder.ServiceLifetime));
            }
        }
    }

    protected void RegisterDefaultRepositories()
    {
        if (!Builder.RegisterDefaultRepositories)
        {
            return;
        }

        var excludedEntityTypes = ReplacedRegistrars
            .SelectMany(x => x.Descriptors)
            .Concat(Descriptors)
            .Where(d => d.EntityType != null)
            .Select(d => d.EntityType!)
            .ToHashSet();

        var entities = GetEntityTypes(DbContextType)
            .Where(t => !excludedEntityTypes.Contains(t))
            .ToList();

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

    protected void ReplaceRepositories()
    {
        foreach (var registrar in ReplacedRegistrars)
        {
            ReplaceRepository(registrar);
        }
    }

    protected void ReplaceRepository(GenericRepositoryRegistrar registrar)
    {
        foreach (var descriptor in registrar.Descriptors)
        {
            if (!descriptor.TenancySide.AppliesTo(Builder.TenancySide))
            {
                continue;
            }

            if (descriptor.IsDefaultRepository)
            {
                ArgumentNullException.ThrowIfNull(descriptor.EntityType);

                descriptor.ImplementationType = descriptor.EntityKeyType == null
                    ? typeof(EfCoreRepository<,>).MakeGenericType(DbContextType, descriptor.EntityType)
                    : typeof(EfCoreRepository<,,>).MakeGenericType(DbContextType, descriptor.EntityType,
                        descriptor.EntityKeyType);
            }
            else
            {
                descriptor.ImplementationType = descriptor.ImplementationType.GetGenericTypeDefinition()
                    .MakeGenericType(DbContextType);
            }

            foreach (var service in descriptor.Services)
            {
                Services.Replace(ServiceDescriptor.Describe(service, descriptor.ImplementationType,
                    registrar.Builder.ServiceLifetime));
            }
        }
    }
}