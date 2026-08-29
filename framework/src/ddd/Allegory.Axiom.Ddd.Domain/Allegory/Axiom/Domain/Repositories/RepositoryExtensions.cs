using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Exceptions;

namespace Allegory.Axiom.Domain.Repositories;

public static class RepositoryExtensions
{
    public const string HardRemoveUnitOfWorkItemKey = $"{nameof(IRepository)}.HardRemove";

    // Create EntityNotFoundException inside Domain package

    extension<TEntity>(IReadOnlyRepository<TEntity> repository) where TEntity : class, IEntity
    {
        public async Task<TEntity> GetAsync(
            Expression<Func<TEntity, bool>> predicate,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var entity = await repository.FindAsync(predicate, includeDetails, cancellationToken);

            return entity ?? throw new NotFoundException();
        }
    }

    extension<TEntity, TKey>(IReadOnlyRepository<TEntity, TKey> repository)
        where TEntity : class, IEntity<TKey>
        where TKey : notnull
    {
        public async Task<TEntity> GetAsync(
            TKey id,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            var entity = await repository.FindAsync(id, includeDetails, cancellationToken);

            return entity ?? throw new NotFoundException();
        }

        public Task<IReadOnlyList<TEntity>> GetPagedListAsync(
            int skip,
            int take,
            Expression<Func<TEntity, bool>>? predicate = null,
            bool includeDetails = false,
            CancellationToken cancellationToken = default)
        {
            return repository.GetPagedListAsync(
                skip,
                take,
                static entities => entities.OrderBy(e => e.Id),
                predicate,
                includeDetails,
                cancellationToken);
        }
    }

    extension<TEntity>(IRepository<TEntity> repository) where TEntity : class, IEntity
    {
        private HashSet<object> GetHardRemoveSet()
        {
            if (repository.UnitOfWork.Items.TryGetValue(HardRemoveUnitOfWorkItemKey, out var value))
            {
                return (HashSet<object>) value;
            }

            var items = new HashSet<object>();
            repository.UnitOfWork.Items[HardRemoveUnitOfWorkItemKey] = items;
            return items;
        }

        public Task HardRemoveAsync(
            TEntity entity,
            bool autoSave = false,
            CancellationToken cancellationToken = default)
        {
            repository.GetHardRemoveSet().Add(entity);
            return repository.RemoveAsync(entity, autoSave, cancellationToken);
        }

        public Task HardRemoveRangeAsync(
            IEnumerable<TEntity> entities,
            bool autoSave = false,
            CancellationToken cancellationToken = default)
        {
            var materialized = entities as ICollection<TEntity> ?? entities.ToList();

            var items = repository.GetHardRemoveSet();
            foreach (var entity in materialized)
            {
                items.Add(entity);
            }

            return repository.RemoveRangeAsync(materialized, autoSave, cancellationToken);
        }
    }
}