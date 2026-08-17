using System;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

internal class GenericRepositoryRegistrar(
    Type dbContextType,
    AxiomDbContextOptionsBuilder builder,
    IServiceCollection services) : RepositoryRegistrarBase(dbContextType, builder, services)
{
    public override void Register()
    {
        throw new NotImplementedException();
    }
}