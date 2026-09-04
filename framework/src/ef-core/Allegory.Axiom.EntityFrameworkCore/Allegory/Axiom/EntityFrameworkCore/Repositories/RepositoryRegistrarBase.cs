using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Allegory.Axiom.Data.ConnectionStrings;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

internal abstract class RepositoryRegistrarBase
{
    protected RepositoryRegistrarBase(
        Type dbContextType,
        AxiomDbContextOptionsBuilder builder,
        IServiceCollection services)
    {
        DbContextType = dbContextType;
        Builder = builder;
        Services = services;

        SetConnectionStringName();
    }

    internal Type DbContextType { get; }
    internal AxiomDbContextOptionsBuilder Builder { get; }
    internal IServiceCollection Services { get; }
    internal TenancySide TenancySide { get; set; }
    internal string ConnectionStringName { get; private set; } = null!;
    internal List<RepositoryDescriptor> Descriptors { get; } = new();

    public abstract void Register();
   
    private void SetConnectionStringName()
    {
        var name = ConnectionStringNameAttribute.Find(DbContextType);
        
        if (name != null)
        {
            ConnectionStringName = name;
            return;
        }

        name = DbContextType.Name;
        ConnectionStringName = name.EndsWith("DbContext", StringComparison.OrdinalIgnoreCase)
            ? name[..^9]
            : name;
    }

    protected static IReadOnlyList<Type> GetEntityTypes(Type type)
    {
        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .Where(p => typeof(IEntity).IsAssignableFrom(p))
            .ToList();
    }
}