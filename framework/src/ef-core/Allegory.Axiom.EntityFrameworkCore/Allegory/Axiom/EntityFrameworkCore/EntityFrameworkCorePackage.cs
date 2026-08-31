using System;
using System.Threading.Tasks;
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