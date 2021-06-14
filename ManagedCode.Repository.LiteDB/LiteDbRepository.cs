using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using ManagedCode.Repository.Core;

namespace ManagedCode.Repository.LiteDB
{
    public class LiteDbRepository<TId, TItem> : BaseRepository<TId, TItem>, ILiteDbRepository<TId, TItem>
        where TItem : LiteDbItem<TId>, IItem<TId>, new()
    {
        private readonly LiteDatabase _database;

        public LiteDbRepository([NotNull] LiteDbRepositoryOptions options)
        {
            _database = options.Database ?? new LiteDatabase(options.ConnectionString);
            IsInitialized = true;
        }

        private ILiteCollection<TItem> GetDatabase()
        {
            return _database.GetCollection<TItem>();
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
            var v = GetDatabase().Insert(item);
            return GetDatabase().FindById(v);
        }

        protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().InsertBulk(items);
        }

        #endregion

        #region InsertOrUpdate

        protected override async Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            await Task.Yield();
            GetDatabase().Upsert(item);
            return GetDatabase().FindById(new BsonValue(item.Id));
        }

        protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().Upsert(items);
        }

        #endregion

        #region Update

        protected override async Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            await Task.Yield();
            if (GetDatabase().Update(item))
            {
                return GetDatabase().FindById(new BsonValue(item.Id));
            }

            return default;
        }

        protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().Update(items);
        }

        #endregion

        #region Delete

        protected override async Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().Delete(new BsonValue(id));
        }

        protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().Delete(new BsonValue(item.Id));
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default)
        {
            await Task.Yield();
            var count = 0;
            var db = GetDatabase();
            foreach (var id in ids)
            {
                if (db.Delete(new BsonValue(id)))
                {
                    count++;
                }
            }

            return count;
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            await Task.Yield();
            var count = 0;
            var db = GetDatabase();
            foreach (var item in items)
            {
                if (db.Delete(new BsonValue(item.Id)))
                {
                    count++;
                }
            }

            return count;
        }

        protected override async Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().DeleteMany(predicate);
        }

        protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().DeleteAll() > 0;
        }

        #endregion

        #region Get

        protected override async Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().FindById(new BsonValue(id));
        }

        protected override async Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().FindOne(predicate);
        }

        protected override async IAsyncEnumerable<TItem> GetAllAsyncInternal(int? take = null,
            int skip = 0,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.Yield();
            var query = GetDatabase().Query().Skip(skip).Limit(take ?? 2147483647);
            foreach (var item in query.ToEnumerable())
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
            var query = GetDatabase().Query();

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
                foreach (var item in query.Limit(take.Value).ToEnumerable())
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
                foreach (var item in query.ToEnumerable())
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
            var query = GetDatabase().Query();

            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }

            ;
            foreach (var item in query.Skip(skip).Limit(take ?? 2147483647).ToEnumerable())
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
            var query = GetDatabase().Query();

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

            if (take != null)
            {
                foreach (var item in query.Skip(skip).Limit(take.Value).ToEnumerable())
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
                foreach (var item in query.Skip(skip).ToEnumerable())
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
            var query = GetDatabase().Query();

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
                foreach (var item in query.Skip(skip).Limit(take.Value).ToEnumerable())
                {
                    yield return item;
                }
            }
            else
            {
                foreach (var item in query.Skip(skip).ToEnumerable())
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
            return GetDatabase().Count();
        }

        protected override async Task<int> CountAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates, CancellationToken token = default)
        {
            await Task.Yield();
            var query = GetDatabase().Query();

            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }

            return query.Count();
        }

        #endregion
    }
}