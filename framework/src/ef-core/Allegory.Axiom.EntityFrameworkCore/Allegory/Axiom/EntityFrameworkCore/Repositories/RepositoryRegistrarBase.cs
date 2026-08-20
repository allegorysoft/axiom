using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Allegory.Axiom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

internal abstract class RepositoryRegistrarBase(
    AxiomDbContextOptionsBuilder builder,
    IServiceCollection services)
{
    internal static Dictionary<Type, RepositoryRegistrar> Registrars { get; } = new();
    internal static Dictionary<Type, GenericRepositoryRegistrar> GenericRegistrars { get; } = new();

    internal AxiomDbContextOptionsBuilder Builder { get; } = builder;
    internal IServiceCollection Services { get; } = services;
    internal List<RepositoryDescriptor> Descriptors { get; } = new();

    public abstract void Register();

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