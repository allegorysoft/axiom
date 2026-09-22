using System.Collections.Generic;

namespace Allegory.Axiom.EntityFrameworkCore;

public interface IModelBuilderContributorProvider
{
    static abstract IList<IModelBuilderContributor> Contributors { get; }
}