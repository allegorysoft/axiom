using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.MultiTenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Data;

public class ConnectionStringProvider : IConnectionStringProvider, ISingletonService
{
    public ConnectionStringProvider(
        IConfiguration configuration,
        IOptions<ConnectionStringOptions> options,
        ITenantContextAccessor tenantContextAccessor)
    {
        Configuration = configuration;
        Options = options.Value;
        TenantContextAccessor = tenantContextAccessor;

        BuildMappings();
    }

    protected IConfiguration Configuration { get; }
    protected ConnectionStringOptions Options { get; }
    protected ITenantContextAccessor TenantContextAccessor { get; }
    protected FrozenDictionary<string, ConnectionStringContextOptions>? Mappings { get; private set; }

    public virtual async ValueTask<string> GetAsync(string name)
    {
        var connectionString = await FindAsync(name);

        return string.IsNullOrWhiteSpace(connectionString)
            ? throw new KeyNotFoundException($"No connection string found for '{name}'.")
            : connectionString;
    }

    public virtual ValueTask<string?> FindAsync(string name)
    {
        var context = Mappings?.GetValueOrDefault(name);

        var connectionString = context == null ? FindByName(name) : FindByContext(context);

        return ValueTask.FromResult(connectionString);
    }

    protected virtual string? FindByName(string name)
    {
        var tenant = TenantContextAccessor.Current;

        if (tenant == null || tenant.ConnectionStrings.Count == 0)
        {
            return FindByConfiguration(name);
        }

        if (tenant.ConnectionStrings.TryGetValue(name, out var connectionString))
        {
            return connectionString;
        }

        if (Options.DefaultName == name)
        {
            return Configuration.GetConnectionString(name);
        }

        if (tenant.ConnectionStrings.TryGetValue(Options.DefaultName, out connectionString))
        {
            return connectionString;
        }

        return Configuration.GetConnectionString(Options.DefaultName);
    }

    protected virtual string? FindByContext(ConnectionStringContextOptions context)
    {
        var tenant = TenantContextAccessor.Current;

        if (context.IsTenantAgnostic || tenant == null || tenant.ConnectionStrings.Count == 0)
        {
            return FindByConfiguration(context.Name);
        }

        if (tenant.ConnectionStrings.TryGetValue(context.Name, out var connectionString))
        {
            return connectionString;
        }

        if (Options.DefaultName == context.Name)
        {
            return Configuration.GetConnectionString(context.Name);
        }

        if (tenant.ConnectionStrings.TryGetValue(Options.DefaultName, out connectionString))
        {
            return connectionString;
        }

        return Configuration.GetConnectionString(Options.DefaultName);
    }

    protected virtual string? FindByConfiguration(string name)
    {
        var connection = Configuration.GetConnectionString(name);

        if (connection != null)
        {
            return connection;
        }

        return Options.DefaultName == name
            ? null
            : Configuration.GetConnectionString(Options.DefaultName);
    }

    private void BuildMappings()
    {
        /*
         {
            "DefaultName": "host-1"
            "Contexts": [
                {
                    "Name": "Administration",
                    "Connections": [PermissionManagement, FeatureManagement, TenantManagement],
                    "IsTenantAgnostic: true
                }
            ]
         */
        if (Options.Contexts == null)
        {
            return;
        }

        var dictionary = new Dictionary<string, ConnectionStringContextOptions>();
        foreach (var context in Options.Contexts)
        {
            dictionary.Add(context.Name, context);

            foreach (var connection in context.Connections)
            {
                dictionary.TryAdd(connection, context);
            }
        }

        Mappings = dictionary.ToFrozenDictionary();
    }
}