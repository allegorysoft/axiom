using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Allegory.Axiom.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    extension<TContext>(IServiceCollection services) where TContext : DbContext
    {
        public void UseMongoDbContextProvider()
        {
            services.TryAddSingleton<IDbContextProvider<TContext>, MongoDbContextProvider<TContext>>();
        }

        public void AddAxiomMongoDbContext(Action<AxiomDbContextOptionsBuilder>? optionsAction = null)
        {
            services.AddAxiomDbContext<TContext>(optionsAction);
            services.UseMongoDbContextProvider<TContext>();
        }
    }
}