using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Repository.Core
{
    public class InMemoryRepository<TId, TItem> : BaseRepository<TId, TItem> where TItem : IItem<TId>
    {
        private readonly Dictionary<TId, TItem> _storage = new();

        public InMemoryRepository()
        {
            IsInitialized = true;
        }

        protected override Task InitializeAsyncInternal(CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        protected override ValueTask DisposeAsyncInternal()
        {
            return new(Task.Run(DisposeInternal));
        }

        protected override void DisposeInternal()
        {
            lock (_storage)
            {
                _storage.Clear();
            }
        }

        #region Insert

        protected override Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default)
        {
            lock (_storage)
            {
                if (!_storage.TryGetValue(item.Id, out var _))
                {
                    _storage.Add(item.Id, item);
                    return Task.FromResult(item);
                }
            }

            return Task.FromResult<TItem>(default);
        }

        protected override Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            var count = 0;

            lock (_storage)
            {
                foreach (var item in items)
                {
                    if (!_storage.TryGetValue(item.Id, out var _))
                    {
                        _storage.Add(item.Id, item);
                        count++;
                    }
                }
            }

            return Task.FromResult(count);
        }

        #endregion

        #region InsertOrUpdate

        protected override Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            lock (_storage)
            {
                _storage[item.Id] = item;
                return Task.FromResult(item);
            }
        }

        protected override Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            var count = 0;

            lock (_storage)
            {
                foreach (var item in items)
                {
                    _storage[item.Id] = item;
                    count++;
                }
            }

            return Task.FromResult(count);
        }

        #endregion

        #region Update

        protected override Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            lock (_storage)
            {
                if (_storage.TryGetValue(item.Id, out var _))
                {
                    _storage[item.Id] = item;
                    return Task.FromResult(item);
                }

                return Task.FromResult<TItem>(default);
            }
        }

        protected override Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            var count = 0;

            lock (_storage)
            {
                foreach (var item in items)
                {
                    if (_storage.TryGetValue(item.Id, out var _))
                    {
                        _storage[item.Id] = item;
                        count++;
                    }
                }
            }

            return Task.FromResult(count);
        }

        #endregion

        #region Delete

        protected override Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default)
        {
            lock (_storage)
            {
                return Task.FromResult(_storage.Remove(id));
            }
        }

        protected override Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
        {
            lock (_storage)
            {
                return Task.FromResult(_storage.Remove(item.Id));
            }
        }

        protected override Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default)
        {
            var count = 0;

            lock (_storage)
            {
                foreach (var id in ids)
                {
                    if (_storage.Remove(id))
                    {
                        count++;
                    }
                }
            }

            return Task.FromResult(count);
        }

        protected override Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            var count = 0;

            lock (_storage)
            {
                foreach (var item in items)
                {
                    if (_storage.Remove(item.Id))
                    {
                        count++;
                    }
                }
            }

            return Task.FromResult(count);
        }

        protected override Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            lock (_storage)
            {
                var count = 0;
                var items = _storage.Values.Where(predicate.Compile());
                foreach (var item in items)
                {
                    if (_storage.Remove(item.Id))
                    {
                        count++;
                    }
                }

                return Task.FromResult(count);
            }
        }

        protected override Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
        {
            lock (_storage)
            {
                var count = _storage.Count;
                _storage.Clear();
                return Task.FromResult(true);
            }
        }

        #endregion

        #region Get

        protected override Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default)
        {
            lock (_storage)
            {
                if (_storage.TryGetValue(id, out var item))
                {
                    return Task.FromResult(item);
                }

                return Task.FromResult<TItem>(default);
            }
        }

        protected override Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            lock (_storage)
            {
                var item = _storage.Values.FirstOrDefault(predicate.Compile());
                return Task.FromResult(item);
            }
        }

        protected override async IAsyncEnumerable<TItem> GetAllAsyncInternal(int? take = null,
            int skip = 0,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.Yield();
            lock (_storage)
            {
                var enumerable = _storage.Values.Skip(skip);

                if (take.HasValue)
                {
                    enumerable = enumerable.Take(take.Value);
                }

                foreach (var item in enumerable)
                {
                    yield return item;
                }
            }
        }

        protected override async IAsyncEnumerable<TItem> GetAllAsyncInternal(Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.Yield();
            lock (_storage)
            {
                IEnumerable<TItem> enumerable;
                if (orderType == Order.By)
                {
                    enumerable = _storage.Values.OrderBy(orderBy.Compile());
                }
                else
                {
                    enumerable = _storage.Values.OrderByDescending(orderBy.Compile());
                }

                enumerable = enumerable.Skip(0);

                if (take.HasValue)
                {
                    enumerable = enumerable.Take(take.Value);
                }

                foreach (var item in enumerable)
                {
                    yield return item;
                }
            }
        }

        #endregion

        #region Find

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
            int? take = null,
            int skip = 0,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.Yield();
            List<TItem> list;
            lock (_storage)
            {
                IEnumerable<TItem> items = null;

                foreach (var predicate in predicates)
                {
                    if (items == null)
                    {
                        items = _storage.Values.Where(predicate.Compile());
                    }
                    else
                    {
                        items = items.Where(predicate.Compile());
                    }
                }

                if (skip != 0)
                {
                    items = items.Skip(skip);
                }

                if (take.HasValue)
                {
                    items = items.Take(take.Value);
                }

                list = items.ToList();
            }

            await Task.Yield();
            foreach (var item in list)
            {
                yield return item;
            }
        }

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.Yield();
            List<TItem> list;
            lock (_storage)
            {
                IEnumerable<TItem> items = null;

                foreach (var predicate in predicates)
                {
                    if (items == null)
                    {
                        items = _storage.Values.Where(predicate.Compile());
                    }
                    else
                    {
                        items = items.Where(predicate.Compile());
                    }
                }

                items = orderType == Order.By ? items.OrderBy(orderBy.Compile()) : items.OrderByDescending(orderBy.Compile());

                if (skip != 0)
                {
                    items = items.Skip(skip);
                }

                if (take.HasValue)
                {
                    items = items.Take(take.Value);
                }

                list = items.ToList();
            }

            await Task.Yield();
            foreach (var item in list)
            {
                yield return item;
            }
        }

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            Order thenType,
            int? take = null,
            int skip = 0,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.Yield();
            List<TItem> list;
            lock (_storage)
            {
                IEnumerable<TItem> items = null;

                foreach (var predicate in predicates)
                {
                    if (items == null)
                    {
                        items = _storage.Values.Where(predicate.Compile());
                    }
                    else
                    {
                        items = items.Where(predicate.Compile());
                    }
                }

                var orderedItems = orderType == Order.By ? items.OrderBy(orderBy.Compile()) : items.OrderByDescending(orderBy.Compile());
                items = thenType == Order.By ? orderedItems.ThenBy(thenBy.Compile()) : orderedItems.ThenByDescending(thenBy.Compile());

                if (skip != 0)
                {
                    items = items.Skip(skip);
                }

                if (take.HasValue)
                {
                    items = items.Take(take.Value);
                }

                list = items.ToList();
            }

            await Task.Yield();
            foreach (var item in list)
            {
                yield return item;
            }
        }

        #endregion

        #region Count

        protected override Task<int> CountAsyncInternal(CancellationToken token = default)
        {
            lock (_storage)
            {
                return Task.FromResult(_storage.Count);
            }
        }

        protected override Task<int> CountAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates, CancellationToken token = default)
        {
            lock (_storage)
            {
                IEnumerable<TItem> items = null;

                foreach (var predicate in predicates)
                {
                    if (items == null)
                    {
                        items = _storage.Values.Where(predicate.Compile());
                    }
                    else
                    {
                        items = items.Where(predicate.Compile());
                    }
                }

                var count = items.Count();
                return Task.FromResult(count);
            }
        }

        #endregion
    }
}