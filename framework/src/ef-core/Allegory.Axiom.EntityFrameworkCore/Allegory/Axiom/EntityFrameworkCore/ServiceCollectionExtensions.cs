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
        internal ExtraProperties GetExtraProperties() => CollectionProperties.GetOrCreateValue(services);

        public void ConfigureAxiomDbContexts(Action<AxiomDbContextsOptions> optionsAction)
        {
            services.Configure(optionsAction);
        }
    }

    extension<TContext>(IServiceCollection services) where TContext : DbContext
    {
        public void AddAxiomDbContext(Action<AxiomDbContextOptionsBuilder>? optionsAction = null)
        {
            var builder = new AxiomDbContextOptionsBuilder(typeof(TContext));
            optionsAction?.Invoke(builder);

            services.RegisterRepositories<TContext>(builder);
            services.RegisterDbContextFactory<TContext>();
            services.ConfigureOptions<TContext>(builder);
        }

        public void ConfigureAxiomDbContext(Action<DbContextOptionsBuilder> optionsAction)
        {
            services.Configure<AxiomDbContextOptions<TContext>>(o => o.BuilderAction = optionsAction);
        }

        public void ReplaceRepository(Type repository)
        {
            var properties = CollectionProperties.GetOrCreateValue(services);
            var registrar = properties.GenericRegistrars[typeof(TContext)];
            registrar.ReplaceRepository(repository);
        }

        private void RegisterRepositories(AxiomDbContextOptionsBuilder builder)
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

        private void RegisterDbContextFactory()
        {
            services.AddDbContextFactory<TContext>(static (sp, o) =>
            {
                var globalOptions = sp.GetRequiredService<IOptions<AxiomDbContextsOptions>>().Value;
                globalOptions.SharedBuilderAction?.Invoke(o);

                var contextOptions = sp.GetRequiredService<IOptions<AxiomDbContextOptions<TContext>>>().Value;
                var builderAction = contextOptions.BuilderAction ?? globalOptions.DefaultBuilderAction;
                builderAction?.Invoke(o);

                o.AddInterceptors(sp.GetRequiredService<AxiomSaveChangesInterceptor>());
            });
        }

        private void ConfigureOptions(AxiomDbContextOptionsBuilder builder)
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
    }

    internal class ExtraProperties
    {
        internal Dictionary<Type, RepositoryRegistrar> Registrars { get; } = new();
        internal Dictionary<Type, GenericRepositoryRegistrar> GenericRegistrars { get; } = new();
    }
}