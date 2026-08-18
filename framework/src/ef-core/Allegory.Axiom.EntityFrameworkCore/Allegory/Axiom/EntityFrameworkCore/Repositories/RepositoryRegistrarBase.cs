using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

internal abstract class RepositoryRegistrarBase(
    Type dbContextType,
    AxiomDbContextOptionsBuilder builder,
    IServiceCollection services)
{
    protected static Dictionary<Type, RepositoryRegistrarBase> Registrars { get; } = new(); // DbContextType, Registrar

    internal Type DbContextType { get; } = dbContextType;
    internal AxiomDbContextOptionsBuilder Builder { get; } = builder;
    internal IServiceCollection Services { get; } = services;
    internal List<RepositoryDescriptor> Descriptors { get; } = new();

    public abstract void Register();

    public static RepositoryRegistrarBase Create(
        Type dbContextType,
        AxiomDbContextOptionsBuilder builder,
        IServiceCollection services)
    {
        return builder.Repositories.Count > 0
            ? new GenericRepositoryRegistrar(dbContextType, builder, services)
            : new RepositoryRegistrar(dbContextType, builder, services);
    }

    protected static IReadOnlyList<Type> GetEntityTypes(Type type)
    {
        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToList();
    }
}