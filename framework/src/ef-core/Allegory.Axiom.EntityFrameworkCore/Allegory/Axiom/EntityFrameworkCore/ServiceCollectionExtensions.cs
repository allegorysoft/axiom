using System;
using Allegory.Axiom.Data;
using Allegory.Axiom.EntityFrameworkCore.Repositories;
using Allegory.Axiom.MultiTenancy;
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
            var builder = new AxiomDbContextOptionsBuilder();
            optionsAction?.Invoke(builder);
            builder.TenancySide ??= TenancySideAttribute.Find(typeof(TContext)) ?? TenancySide.Hybrid;
            builder.ConnectionStringName ??= ConnectionStringNameAttribute.Find(typeof(TContext));
            builder.ReplacedDbContexts ??= ReplaceDbContextAttribute.Find(typeof(TContext));

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
            o.TenancySide = builder.TenancySide!.Value;
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
        var registrar = RepositoryRegistrarBase.Create(typeof(TContext), builder, services);
        registrar.Register();
    }
}