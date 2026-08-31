using System;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Data;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.EntityFrameworkCore.Repositories;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EntityFrameworkCore;

internal sealed class EntityFrameworkCorePackage : IConfigureApplication
{
    static  EntityFrameworkCorePackage()
    {
        AssemblyDependencyRegistrar.IgnoredServiceTypes.Add(typeof(IInterceptor));
        AssemblyDependencyRegistrar.IgnoredServiceTypes.Add(typeof(ISaveChangesInterceptor));
    }

    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.AddDeferredAction(CompleteRepositoryRegistration);

        return Task.CompletedTask;
    }

    private static void CompleteRepositoryRegistration(IHostApplicationBuilder builder)
    {
        var properties = builder.Services.GetExtraProperties();

        foreach (var (type, registrar) in properties.GenericRegistrars)
        {
            registrar.Register();
            ConfigureOptions(builder.Services, type, registrar);
        }

        foreach (var (type, registrar) in properties.Registrars)
        {
            registrar.Register();
            ConfigureOptions(builder.Services, type, registrar);
        }

        ConfigureConnectionStringOptions(builder);
    }
    
    private static void ConfigureOptions(
        IServiceCollection services,
        Type contextType,
        RepositoryRegistrarBase registrar)
    {
        services.Configure<AxiomDbContextsOptions>(o => o.AddContext(contextType));

        var optionsType = typeof(AxiomDbContextOptions<>).MakeGenericType(contextType);
        var configureOptionsType = typeof(IConfigureOptions<>).MakeGenericType(optionsType);

        services.AddSingleton(configureOptionsType,
            Activator.CreateInstance(
                typeof(AxiomDbContextOptionsConfigurer<>).MakeGenericType(contextType),
                registrar.Builder.BuilderAction,
                registrar.ConnectionStringName,
                registrar.TenancySide)!);
    }

    private static void ConfigureConnectionStringOptions(IHostApplicationBuilder builder)
    {
        var properties = builder.Services.GetExtraProperties();
 
        var registrars = properties.Registrars;
        foreach (var (_, registrar) in registrars)
        {
            var context = new ConnectionStringContextOptions
            {
                Name = registrar.ConnectionStringName,
                IsTenantAgnostic = registrar.TenancySide == TenancySide.Host
            };

            builder.Services.Configure<ConnectionStringContextsOptions>(o =>
            {
                o.Contexts.Add(context);
            });
        }

        var replacedContexts = registrars
            .Select(r => r.Value.ReplacedRegistrars.Select(d => d.DbContextType))
            .SelectMany(r => r)
            .Distinct()
            .ToList();

        var genericRegistrars = properties.GenericRegistrars.Where(g => !replacedContexts.Contains(g.Key)).ToList();
        foreach (var (_, registrar) in genericRegistrars)
        {
            var context = new ConnectionStringContextOptions
            {
                Name = registrar.ConnectionStringName,
                IsTenantAgnostic = registrar.TenancySide == TenancySide.Host
            };

            builder.Services.Configure<ConnectionStringContextsOptions>(o =>
            {
                if (!o.Contexts.SelectMany(c => c.Connections).Any(f => f == context.Name))
                {
                    o.Contexts.Add(context);    
                }
            });
        }
    }
    
    private sealed class AxiomDbContextOptionsConfigurer<TContext>(
        Action<DbContextOptionsBuilder>? builderAction,
        string connectionStringName,
        TenancySide tenancySide) :
        IConfigureOptions<AxiomDbContextOptions<TContext>>
        where TContext : DbContext
    {
        public void Configure(AxiomDbContextOptions<TContext> o)
        {
            o.BuilderAction ??= builderAction;
            o.ConnectionStringName = connectionStringName;
            o.TenancySide = tenancySide;
        }
    }
}