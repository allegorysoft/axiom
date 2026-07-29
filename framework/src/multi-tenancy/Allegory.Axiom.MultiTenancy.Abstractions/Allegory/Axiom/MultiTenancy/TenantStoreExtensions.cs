using System;
using System.Threading.Tasks;
using Allegory.Axiom.Exceptions;

namespace Allegory.Axiom.MultiTenancy;

public static class TenantStoreExtensions
{
    extension(ITenantStore store)
    {
        public async ValueTask<TenantContext> GetAsync(Guid id)
        {
            var tenant = await store.FindAsync(id);

            if (tenant == null)
            {
                throw new NotFoundException(MultiTenancyExceptionCodes.TenantNotFound)
                    .AddData("identifier", id);
            }

            return tenant;
        }

        public async ValueTask<TenantContext> GetAsync(string name)
        {
            var tenant = await store.FindAsync(name);

            if (tenant == null)
            {
                throw new NotFoundException(MultiTenancyExceptionCodes.TenantNotFound)
                    .AddData("identifier", name);
            }

            return tenant;
        }
    }
}