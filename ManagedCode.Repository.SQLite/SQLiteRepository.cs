using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Repository.Core;
using SQLite;

namespace ManagedCode.Repository.SQLite
{
    public class SQLiteRepository<TId, TItem> : BaseRepository<TId, TItem> where TItem : class, IItem<TId>, new()
    {
        private readonly SQLiteConnection _database;

        public SQLiteRepository(
            [System.Diagnostics.CodeAnalysis.NotNull]
            SQLiteRepositoryOptions options)
        {
            _database = options.Connection ?? new SQLiteConnection(options.ConnectionString);
            _database.CreateTable<TItem>();
            IsInitialized = true;
        }

        protected override Task InitializeAsyncInternal(CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        protected override ValueTask DisposeAsyncInternal()
        {
            return new(Task.CompletedTask);
        }

        protected override void DisposeInternal()
        {
        }

        #region Insert

        protected override async Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default)
        {
            await Task.Yield();
            var v = _database.Insert(item);
            return item;
        }

        protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            await Task.Yield();
            return _database.InsertAll(items);
        }

        #endregion

        #region InsertOrUpdate

        protected override async Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            await Task.Yield();
            _database.InsertOrReplace(item);
            return item;
        }

        protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            await Task.Yield();
            var count = 0;
            foreach (var item in items)
            {
                count += _database.InsertOrReplace(item);
            }

            return count;
        }

        #endregion

        #region Update

        protected override async Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            await Task.Yield();
            _database.Update(item);
            return item;
        }

        protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            await Task.Yield();
            var count = 0;
            foreach (var item in items)
            {
                count += _database.Update(item);
            }

            return count;
        }

        #endregion

        #region Delete

        protected override async Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default)
        {
            await Task.Yield();
            return _database.Delete<TItem>(id) != 0;
        }

        protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
        {
            await Task.Yield();
            return _database.Delete(item) != 0;
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default)
        {
            await Task.Yield();
            var count = 0;
            foreach (var id in ids)
            {
                count += _database.Delete<TItem>(id);
            }

            return count;
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            await Task.Yield();
            var count = 0;
            foreach (var item in items)
            {
                count += _database.Delete<TItem>(item.Id);
            }

            return count;
        }

        protected override async Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            await Task.Yield();
            var ids = _database.Table<TItem>().Where(predicate).Select(s => s.Id);
            return await DeleteAsyncInternal(ids, token);
        }

        protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
        {
            await Task.Yield();
            return _database.DeleteAll<TItem>() != 0;
        }

        #endregion

        #region Get

        protected override async Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default)
        {
            await Task.Yield();
            return _database.Find<TItem>(id);
        }

        protected override async Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            await Task.Yield();
            return _database.Table<TItem>().Where(predicate).FirstOrDefault();
        }

        protected override async IAsyncEnumerable<TItem> GetAllAsyncInternal(int? take = null,
            int skip = 0,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.Yield();
            var query = _database.Table<TItem>().Skip(skip);
            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            foreach (var item in query)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                yield return item;
            }
        }

        protected override async IAsyncEnumerable<TItem> GetAllAsyncInternal(Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.Yield();
            var query = _database.Table<TItem>();

            if (orderType == Order.By)
            {
                query = query.OrderBy(orderBy);
            }
            else
            {
                query.OrderByDescending(orderBy);
            }

            if (take != null)
            {
                foreach (var item in query.Take(take.Value))
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    yield return item;
                }
            }
            else
            {
                foreach (var item in query)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

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
            var query = _database.Table<TItem>();

            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }

            query = query.Skip(skip);

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            foreach (var item in query)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

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
            var query = _database.Table<TItem>();

            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }

            if (orderType == Order.By)
            {
                query = query.OrderBy(orderBy);
            }
            else
            {
                query.OrderByDescending(orderBy);
            }

            query = query.Skip(skip);

            if (take != null)
            {
                foreach (var item in query.Take(take.Value))
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    yield return item;
                }
            }
            else
            {
                foreach (var item in query)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    yield return item;
                }
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
            var query = _database.Table<TItem>();

            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }

            if (orderType == Order.By)
            {
                query = query.OrderBy(orderBy);
            }
            else
            {
                query.OrderByDescending(orderBy);
            }

            if (thenType == Order.By)
            {
                query = query.OrderBy(thenBy);
            }
            else
            {
                query.OrderByDescending(thenBy);
            }

            if (take != null)
            {
                foreach (var item in query.Skip(skip).Take(take.Value))
                {
                    yield return item;
                }
            }
            else
            {
                foreach (var item in query.Skip(skip))
                {
                    yield return item;
                }
            }
        }

        #endregion

        #region Count

        protected override async Task<int> CountAsyncInternal(CancellationToken token = default)
        {
            await Task.Yield();
            return _database.Table<TItem>().Count();
        }

        protected override async Task<int> CountAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates, CancellationToken token = default)
        {
            await Task.Yield();
            var query = _database.Table<TItem>();

            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }

            return query.Count();
        }

        #endregion
    }
}