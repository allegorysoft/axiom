using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Allegory.Axiom.EntityFrameworkCore.Interceptors;
using Allegory.Axiom.EntityFrameworkCore.Repositories;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    internal static readonly ConditionalWeakTable<IServiceCollection, ExtraProperties> CollectionProperties = new();

    extension(IServiceCollection services)
    {
        public void AddAxiomDbContext<TContext>(
            Action<AxiomDbContextOptionsBuilder>? optionsAction = null)
            where TContext : DbContext
        {
            var builder = new AxiomDbContextOptionsBuilder(typeof(TContext));
            optionsAction?.Invoke(builder);

            ConfigureOptions<TContext>(services, builder);
            RegisterDbContextFactory<TContext>(services);
            RegisterRepositories<TContext>(services, builder);
        }

        public void ConfigureAxiomDbContext<TContext>(
            Action<DbContextOptionsBuilder> optionsAction)
            where TContext : DbContext
        {
            services.Configure<AxiomDbContextOptions<TContext>>(o => o.BuilderAction = optionsAction);
        }

        public void ConfigureAxiomDbContexts(Action<AxiomDbContextsOptions> optionsAction)
        {
            services.Configure(optionsAction);
        }

        public void ReplaceRepository<TContext>(Type repository, TenancySide? tenancySide = null) where TContext : DbContext
        {
            var properties = CollectionProperties.GetOrCreateValue(services);
            var registrar = properties.GenericRegistrars[typeof(TContext)];
            registrar.ReplaceRepository(repository, tenancySide);
        }
    }

    private static void ConfigureOptions<TContext>(
        IServiceCollection services,
        AxiomDbContextOptionsBuilder builder)
        where TContext : DbContext
    {
        services.Configure<AxiomDbContextsOptions>(o => o.AddContext(typeof(TContext)));

        services.Configure<AxiomDbContextOptions<TContext>>(o =>
        {
            o.BuilderAction ??= builder.BuilderAction; // ConfigureAxiomDbContext might run first
            o.TenancySide = builder.TenancySide;
            o.ConnectionStringName = builder.ConnectionStringName;
            o.ReplacedDbContexts = builder.ReplacedDbContexts;
        });
    }

    private static void RegisterDbContextFactory<TContext>(IServiceCollection services) where TContext : DbContext
    {
        services.AddDbContextFactory<TContext>(static (sp, o) =>
        {
            var globalOptions = sp.GetRequiredService<IOptions<AxiomDbContextsOptions>>().Value;
            globalOptions.SharedBuilderAction?.Invoke(o);

            var contextOptions = sp.GetRequiredService<IOptions<AxiomDbContextOptions<TContext>>>().Value;
            var builderAction = contextOptions.BuilderAction ?? globalOptions.DefaultBuilderAction;
            builderAction?.Invoke(o);

            o.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
            o.AddInterceptors(sp.GetRequiredService<EntityEventPublisherInterceptor>());
        });
    }

    private static void RegisterRepositories<TContext>(
        IServiceCollection services,
        AxiomDbContextOptionsBuilder builder)
    {
        var properties = CollectionProperties.GetOrCreateValue(services);

        if (builder.Repositories.Count > 0 || builder.RegisterAsGenericDbContext)
        {
            var registrar = new GenericRepositoryRegistrar(builder, services);
            registrar.Register();
            properties.GenericRegistrars[typeof(TContext)] = registrar;
        }
        else
        {
            var registrar = new RepositoryRegistrar(builder, services);
            properties.Registrars[typeof(TContext)] = registrar;
        }
    }

    internal class ExtraProperties
    {
        internal Dictionary<Type, RepositoryRegistrar> Registrars { get; } = new();
        internal Dictionary<Type, GenericRepositoryRegistrar> GenericRegistrars { get; } = new();
    }
}