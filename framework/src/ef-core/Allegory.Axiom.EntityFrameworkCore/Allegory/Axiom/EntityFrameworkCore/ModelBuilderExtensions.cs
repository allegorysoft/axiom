using System.Linq.Expressions;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    extension(ModelBuilder builder)
    {
        public void ConfigureAxiom()
        {
            // We should call replaced db contexts configure methods in here automatically

            foreach (var entity in builder.Model.GetEntityTypes()) // IMutableEntityType
            {
                // builder.Entity(entity.ClrType, entityBuilder => // EntityTypeBuilder
                // { 
                //     entityBuilder.HasQueryFilter("a", () => true);
                // });

                if (typeof(ISoftDelete).IsAssignableFrom(entity.ClrType))
                {
                    var parameter = Expression.Parameter(entity.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                    var condition = Expression.Equal(property, Expression.Constant(false));
                    var lambda = Expression.Lambda(condition, parameter);

                    entity.SetQueryFilter(nameof(ISoftDelete), lambda);
                }

                if (typeof(ITenantOwned).IsAssignableFrom(entity.ClrType))
                {
                    // CurrentTenantId = ITenantContextAccessor.TryGetCurrent()?.Id
                    // Expression.Call(ITenantContextAccessor.TryGetCurrent) take this as parameter not constant
                    // Add filter
                }
            }
        }
    }
}