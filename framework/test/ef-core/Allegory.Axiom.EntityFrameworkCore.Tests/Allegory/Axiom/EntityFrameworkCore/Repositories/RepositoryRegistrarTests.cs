using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.EntityFrameworkCore.DbContexts;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.EntityFrameworkCore.Repositories;

public class RepositoryRegistrarTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task ShouldRegisterRepositories()
    {
        var repository1 = fixture.Service<IModule1Entity1Repository>();
        var repository2 = fixture.Service<IRepository<Module2Entity1, int>>();
        var repository3 = fixture.Service<IRepository<Module3Entity1, int>>();

        repository1.GetType().GetGenericArguments().First().ShouldBe(typeof(Module1DbContext));
        repository2.GetType().GetGenericArguments().First().ShouldBe(typeof(Module2DbContext));
        repository3.GetType().GetGenericArguments().First().ShouldBe(typeof(Module3DbContext));
    }
}

[ReplaceDbContext(typeof(Module1DbContext), typeof(Module2DbContext), typeof(Module3DbContext))]
file class AppDbContext1 : DbContext
{
}