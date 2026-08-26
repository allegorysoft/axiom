using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.EntityFrameworkCore.DbContexts;
using Allegory.Axiom.EntityFrameworkCore.Repositories;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.Priority;
using DiffEngine;
using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Allegory.Axiom.EntityFrameworkCore;

internal sealed class EntityFrameworkCoreTestsPackage : IConfigureApplication
{
    public static async Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        await ConfigureDatabaseAsync(builder);

        builder.Services.ConfigureAxiomDbContexts(o => { o.DefaultBuilderAction = b => { b.UseSqlite(); }; });

        builder.AddDeferredAction(RemoveUnconfiguredDbContextRepositories, PriorityLevel.Low);
    }

    private static async Task ConfigureDatabaseAsync(IHostApplicationBuilder builder)
    {
        var container = new PostgreSqlBuilder("postgres:16")
            .WithUsername("admin")
            .WithPassword("admin")
            .Build();

        await builder.AddTestContainerAsync(container);

        var app2ConnectionStringBuilder = new NpgsqlConnectionStringBuilder(container.GetConnectionString())
        {
            Database = "app2"
        };

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:App2"] = app2ConnectionStringBuilder.ConnectionString
        });
    }

    private static void RemoveUnconfiguredDbContextRepositories(IHostApplicationBuilder builder)
    {
        // The non-generic repository registrar registers repositories for all DbContexts
        // in the specified assembly. If multiple DbContexts exist but only one is registered
        // with AddAxiomDbContext, repositories for the unregistered DbContexts are also added
        // to DI. This causes DI validation to fail because their corresponding DbContexts
        // were never registered. Remove those repositories when their DbContext was not
        // configured with AddAxiomDbContext.
        var properties = ServiceCollectionExtensions.CollectionProperties.GetOrCreateValue(builder.Services);
        var registrar = properties.Registrars.FirstOrDefault().Value;

        if (registrar == null)
        {
            return;
        }

        RemoveRepositoriesFor<App1DbContext>(properties, registrar, builder, typeof(App1Entity1));
        RemoveRepositoriesFor<App2DbContext>(properties, registrar, builder, typeof(App2Entity1));
    }

    private static void RemoveRepositoriesFor<T>(
        ServiceCollectionExtensions.ExtraProperties properties,
        RepositoryRegistrar registrar,
        IHostApplicationBuilder builder,
        params IEnumerable<Type> entityTypes)
    {
        if (properties.Registrars.ContainsKey(typeof(T)))
        {
            return;
        }

        foreach (var descriptor in registrar.Descriptors.Where(t => entityTypes.Contains(t.EntityType)))
        {
            foreach (var service in descriptor.Services)
            {
                builder.Services.Remove(builder.Services.Single(s => s.ServiceType == service));
            }
        }
    }
}