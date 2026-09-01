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
            var builder = new AxiomDbContextOptionsBuilder();
            optionsAction?.Invoke(builder);

            services.RegisterRepositories<TContext>(builder);
            services.RegisterDbContextFactory<TContext>();
        }

        public void ConfigureAxiomDbContext(Action<AxiomDbContextOptions<TContext>> optionsAction)
        {
            services.Configure(optionsAction);
        }

        public void ReplaceRepository(Type repository)
        {
            var properties = services.GetExtraProperties();
            var registrar = properties.GenericRegistrars[typeof(TContext)];
            registrar.ReplaceRepository(repository);
        }

        private void RegisterRepositories(AxiomDbContextOptionsBuilder builder)
        {
            var properties = CollectionProperties.GetOrCreateValue(services);

            if (builder.RegisterAsGenericDbContext)
            {
                properties.GenericRegistrars[typeof(TContext)] =
                    new GenericRepositoryRegistrar(typeof(TContext), builder, services);
            }
            else
            {
                properties.Registrars[typeof(TContext)] = new RepositoryRegistrar(typeof(TContext), builder, services);
            }
        }

        private void RegisterDbContextFactory()
        {
            services.AddDbContextFactory<TContext>(static (sp, o) =>
            {
                var globalOptions = sp.GetRequiredService<IOptions<AxiomDbContextsOptions>>().Value;
                globalOptions.SharedBuilderAction?.Invoke(sp, o);

                var contextOptions = sp.GetRequiredService<IOptions<AxiomDbContextOptions<TContext>>>().Value;
                var builderAction = contextOptions.BuilderAction ?? globalOptions.BuilderAction;
                builderAction?.Invoke(sp, o);

                o.AddInterceptors(sp.GetRequiredService<AxiomSaveChangesInterceptor>());
            });
        }
    }

    internal class ExtraProperties
    {
        internal Dictionary<Type, RepositoryRegistrar> Registrars { get; } = new();
        internal Dictionary<Type, GenericRepositoryRegistrar> GenericRegistrars { get; } = new();
    }
}