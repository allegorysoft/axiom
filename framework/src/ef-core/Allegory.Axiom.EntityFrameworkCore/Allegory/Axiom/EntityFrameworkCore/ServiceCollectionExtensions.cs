using System;
using Allegory.Axiom.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
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

        public void ConfigureAxiomDbContextGlobalOptions(Action<AxiomDbContextGlobalOptions> optionsAction)
        {
            services.Configure(optionsAction);
        }
    }

    private static void ConfigureOptions<TContext>(
        IServiceCollection services,
        AxiomDbContextOptionsBuilder builder)
        where TContext : DbContext
    {
        services.Configure<AxiomDbContextGlobalOptions>(o => o.Contexts.Add(typeof(TContext)));

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
            var globalOptions = sp.GetRequiredService<IOptions<AxiomDbContextGlobalOptions>>().Value;
            globalOptions.SharedBuilderAction?.Invoke(o);

            var contextOptions = sp.GetRequiredService<IOptions<AxiomDbContextOptions<TContext>>>().Value;
            var builderAction = contextOptions.BuilderAction ?? globalOptions.DefaultBuilderAction;
            builderAction?.Invoke(o);
        });
    }

    private static void RegisterRepositories<TContext>(
        IServiceCollection services,
        AxiomDbContextOptionsBuilder builder)
    {
        if (builder.Repositories.Count > 0 || builder.RegisterAsGenericDbContext)
        {
            var registrar = new GenericRepositoryRegistrar(builder, services);
            registrar.Register();
            RepositoryRegistrarBase.GenericRegistrars[typeof(TContext)] = registrar;
        }
        else
        {
            var registrar = new RepositoryRegistrar(builder, services);
            RepositoryRegistrarBase.Registrars[typeof(TContext)] = registrar;
        }
    }
}