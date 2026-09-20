using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore.ModelBuilding;

public interface IModelBuilderContributor
{
    void Contribute(ModelBuilder modelBuilder, DbContext dbContext, bool createIndexes);
}