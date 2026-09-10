using System.Collections.Frozen;
using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.MultiTenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Data.ConnectionStrings;

public class ConnectionStringProvider : IConnectionStringProvider, ISingletonService
{
    public ConnectionStringProvider(
        IConfiguration configuration,
        IOptions<ConnectionStringContextsOptions> options,
        ITenantContextAccessor tenantContextAccessor)
    {
        TenantContextAccessor = tenantContextAccessor;

        BuildContexts(options.Value.Contexts, configuration);
    }

    public FrozenDictionary<string, ConnectionStringContextOptions> Contexts { get; private set; } = null!;

    protected FrozenDictionary<string, string> ConnectionStrings { get; private set; } = null!;
    protected ITenantContextAccessor TenantContextAccessor { get; }

    public virtual async ValueTask<string> GetAsync(string name)
    {
        var connectionString = await FindAsync(name);

        return string.IsNullOrWhiteSpace(connectionString)
            ? throw new KeyNotFoundException($"No connection string found for '{name}'.")
            : connectionString;
    }

    public virtual ValueTask<string?> FindAsync(string name)
    {
        var context = Contexts.GetValueOrDefault(name);

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
        return ConnectionStrings.GetValueOrDefault(name);
    }

    protected virtual string? FindByTenant(TenantContext tenant, string name)
    {
        if (tenant.ConnectionStrings.TryGetValue(name, out var connectionString) ||
            tenant.ConnectionStrings.TryGetValue(IConnectionStringProvider.DefaultName, out connectionString))
        {
            return connectionString;
        }

        return FindByConfiguration(name);
    }

    private void BuildContexts(HashSet<ConnectionStringContextOptions> contexts, IConfiguration configuration)
    {
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
        
        // Avoid redundant heap allocation from IConfiguration.GetConnectionString
        var connectionStringsSection = configuration.GetSection("ConnectionStrings");
        ConnectionStrings = connectionStringsSection
            .GetChildren()
            .ToFrozenDictionary(x => x.Key, x => x.Value ?? string.Empty);
    }
}