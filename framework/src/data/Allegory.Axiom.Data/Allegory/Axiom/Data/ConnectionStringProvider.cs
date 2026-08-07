using System.Collections.Frozen;
using System.Collections.Generic;
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
        IOptions<ConnectionStringContextsOptions> options,
        ITenantContextAccessor tenantContextAccessor)
    {
        Configuration = configuration;
        TenantContextAccessor = tenantContextAccessor;

        BuildContexts(options.Value.Contexts);
    }

    protected IConfiguration Configuration { get; }
    protected ITenantContextAccessor TenantContextAccessor { get; }
    protected FrozenDictionary<string, ConnectionStringContextOptions>? Contexts { get; private set; }

    public virtual async ValueTask<string> GetAsync(string name = IConnectionStringProvider.DefaultName)
    {
        var connectionString = await FindAsync(name);

        return string.IsNullOrWhiteSpace(connectionString)
            ? throw new KeyNotFoundException($"No connection string found for '{name}'.")
            : connectionString;
    }

    public virtual ValueTask<string?> FindAsync(string name = IConnectionStringProvider.DefaultName)
    {
        var context = Contexts?.GetValueOrDefault(name);

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

        return FindByTenant(tenant, name);
    }

    protected virtual string? FindByContext(ConnectionStringContextOptions context)
    {
        var tenant = TenantContextAccessor.Current;

        if (context.IsTenantAgnostic || tenant == null || tenant.ConnectionStrings.Count == 0)
        {
            return FindByConfiguration(context.Name);
        }

        return FindByTenant(tenant, context.Name);
    }

    protected virtual string? FindByConfiguration(string name)
    {
        var connection = Configuration.GetConnectionString(name);

        if (connection != null)
        {
            return connection;
        }

        return name == IConnectionStringProvider.DefaultName
            ? null
            : Configuration.GetConnectionString(IConnectionStringProvider.DefaultName);
    }

    protected virtual string? FindByTenant(TenantContext tenant, string name)
    {
        if (tenant.ConnectionStrings.TryGetValue(name, out var connectionString))
        {
            return connectionString;
        }

        if (name == IConnectionStringProvider.DefaultName)
        {
            return Configuration.GetConnectionString(name);
        }

        if (tenant.ConnectionStrings.TryGetValue(IConnectionStringProvider.DefaultName, out connectionString))
        {
            return connectionString;
        }

        return Configuration.GetConnectionString(IConnectionStringProvider.DefaultName);
    }

    private void BuildContexts(HashSet<ConnectionStringContextOptions>? contexts)
    {
        if (contexts == null)
        {
            return;
        }

        var dictionary = new Dictionary<string, ConnectionStringContextOptions>();
        foreach (var context in contexts)
        {
            dictionary.Add(context.Name, context);

            foreach (var connection in context.Connections)
            {
                dictionary.TryAdd(connection, context);
            }
        }

        Contexts = dictionary.ToFrozenDictionary();
    }
}