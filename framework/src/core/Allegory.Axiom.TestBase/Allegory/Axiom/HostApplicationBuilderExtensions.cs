using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom;

public static class HostApplicationBuilderExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers the container.
        /// </summary>
        public void AddTestContainer(IContainer container)
        {
            builder.Services.AddSingleton(new TestContainer(container));
        }

        /// <summary>
        /// Registers the container and starts it.
        /// </summary>
        public async Task AddTestContainerAsync(
            IContainer container,
            CancellationToken cancellationToken = default)
        {
            await container.StartAsync(cancellationToken);
            builder.Services.AddSingleton(new TestContainer(container));
        }
    }
}