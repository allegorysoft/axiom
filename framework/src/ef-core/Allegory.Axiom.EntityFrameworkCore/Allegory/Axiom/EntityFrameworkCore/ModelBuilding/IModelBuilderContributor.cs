using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public interface IModelBuilderContributor
{
    void Contribute(ModelBuilder modelBuilder, DbContext dbContext);
}