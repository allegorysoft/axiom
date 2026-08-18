using System;
using System.Collections.Generic;
using System.Linq;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

internal class RepositoryRegistrar(
    Type dbContextType,
    AxiomDbContextOptionsBuilder builder,
    IServiceCollection services) :
    RepositoryRegistrarBase(dbContextType, builder, services)
{
    public override void Register()
    {
        Registrars[DbContextType] = this;

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

        var entities = GetEntityTypes(DbContextType).Where(t => Descriptors.All(d => t != d.EntityType)).ToList();

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
        if (Builder.ReplacedDbContexts == null || Builder.ReplacedDbContexts.Count == 0)
        {
            return;
        }

        foreach (var dbContextType in Builder.ReplacedDbContexts)
        {
            if (!Registrars.TryGetValue(dbContextType, out var registrar))
            {
                continue;
            }

            ReplaceRepository(registrar);
        }
    }

    protected void ReplaceRepository(RepositoryRegistrarBase registrar)
    {
        foreach (var descriptor in registrar.Descriptors)
        {
            //descriptor.ImplementationType.GetGenericTypeDefinition().MakeGenericType
            //descriptor.ImplementationType = ;
        }
    }
}