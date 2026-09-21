using System.Collections.Generic;

namespace Allegory.Axiom.EntityFrameworkCore.ModelBuilding;

public interface IModelBuilderContributorProvider
{
    static abstract IList<IModelBuilderContributor> Contributors { get; }
}