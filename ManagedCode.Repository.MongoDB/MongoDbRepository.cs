using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using ManagedCode.Repository.Core;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ManagedCode.Repository.MongoDB
{
    public class MongoDbRepository<TItem> : BaseRepository<ObjectId, TItem>, IMongoDbRepository<TItem>
        where TItem : class, IItem<ObjectId>
    {
        private readonly IMongoCollection<TItem> _collection;

        public MongoDbRepository([NotNull] MongoDbRepositoryOptions options)
        {
            var client = new MongoClient(options.ConnectionString);
            var database = client.GetDatabase(options.DataBaseName);
            var collectionName = string.IsNullOrEmpty(options.CollectionName) ? typeof(TItem).Name.Pluralize() : options.CollectionName;
            _collection = database.GetCollection<TItem>(collectionName, new MongoCollectionSettings());
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
            await _collection.InsertOneAsync(item, new InsertOneOptions(), token);
            return item;
        }

        protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            await _collection.InsertManyAsync(items, new InsertManyOptions(), token);
            return items.Count();
        }

        #endregion

        #region InsertOrUpdate

        protected override async Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            var result = await _collection.ReplaceOneAsync(w => w.Id == item.Id, item, new ReplaceOptions
            {
                IsUpsert = true
            }, token);

            return item;
        }

        protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            var count = 0;
            foreach (var item in items)
            {
                await InsertOrUpdateAsyncInternal(item, token);
                count++;
            }

            return count;
        }

        #endregion

        #region Update

        protected override async Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            var r = await _collection.ReplaceOneAsync(Builders<TItem>.Filter.Eq("_id", item.Id), item, cancellationToken: token);
            return item;
        }

        protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            var count = 0;
            foreach (var item in items)
            {
                await UpdateAsyncInternal(item, token);
                count++;
            }

            return count;
        }

        #endregion

        #region Delete

        protected override async Task<bool> DeleteAsyncInternal(ObjectId id, CancellationToken token = default)
        {
            var item = await _collection.FindOneAndDeleteAsync<TItem>(w => w.Id == id, new FindOneAndDeleteOptions<TItem>(), token);
            return item != null;
        }

        protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
        {
            var i = await _collection.FindOneAndDeleteAsync<TItem>(w => w.Id == item.Id, new FindOneAndDeleteOptions<TItem>(), token);
            return i != null;
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<ObjectId> ids, CancellationToken token = default)
        {
            var count = 0;
            foreach (var item in ids)
            {
                if (await DeleteAsyncInternal(item, token))
                {
                    count++;
                }
            }

            return count;
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            var count = 0;
            foreach (var item in items)
            {
                if (await DeleteAsyncInternal(item, token))
                {
                    count++;
                }
            }

            return count;
        }

        protected override async Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            var result = await _collection.DeleteManyAsync<TItem>(predicate, token);
            return Convert.ToInt32(result.DeletedCount);
        }

        protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
        {
            var result = await _collection.DeleteManyAsync(w => true, token);
            return result.DeletedCount > 0;
        }

        #endregion

        #region Get

        protected override async Task<TItem> GetAsyncInternal(ObjectId id, CancellationToken token = default)
        {
            var cursor = await _collection.FindAsync(w => w.Id == id, cancellationToken: token);
            return cursor.FirstOrDefault();
        }

        protected override async Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            var cursor = await _collection.FindAsync(predicate, cancellationToken: token);
            return cursor.FirstOrDefault();
        }

        protected override async IAsyncEnumerable<TItem> GetAllAsyncInternal(int? take = null,
            int skip = 0,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            var query = _collection.AsQueryable().Skip(skip);
            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            foreach (var item in await Task.Run(() => query.ToArray(), token))
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
            IQueryable<TItem> query = _collection.AsQueryable();

            if (orderType == Order.By)
            {
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(orderBy);
            }

            if (take != null)
            {
                foreach (var item in await Task.Run(() => query.Take(take.Value).ToArray(), token))
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
                foreach (var item in await Task.Run(() => query.ToArray(), token))
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
            IQueryable<TItem> query = _collection.AsQueryable();

            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }
            
            if (take != null)
            {
                foreach (var item in await Task.Run(() => query.Skip(skip).Take(take.Value).ToArray(), token))
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
                foreach (var item in await Task.Run(() => query.Skip(skip).ToArray(), token))
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
            int? take = null,
            int skip = 0,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            IQueryable<TItem> query = _collection.AsQueryable();

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
                query = query.OrderByDescending(orderBy);
            }

            if (take != null)
            {
                foreach (var item in await Task.Run(() => query.Skip(skip).Take(take.Value).ToArray(), token))
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
                foreach (var item in await Task.Run(() => query.Skip(skip).ToArray(), token))
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
            IQueryable<TItem> query = _collection.AsQueryable();

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
                query = query.OrderByDescending(orderBy);
            }

            if (thenType == Order.By)
            {
                query = query.OrderBy(thenBy);
            }
            else
            {
                query = query.OrderByDescending(thenBy);
            }

            if (take != null)
            {
                foreach (var item in await Task.Run(() => query.Skip(skip).Take(take.Value).ToArray(), token))
                {
                    yield return item;
                }
            }
            else
            {
                foreach (var item in await Task.Run(() => query.Skip(skip).ToArray(), token))
                {
                    yield return item;
                }
            }
        }

        #endregion

        #region Count

        protected override async Task<int> CountAsyncInternal(CancellationToken token = default)
        {
            return Convert.ToInt32(await _collection.CountDocumentsAsync(f => true, new CountOptions(), token));
        }

        protected override async Task<int> CountAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates, CancellationToken token = default)
        {
            IQueryable<TItem> query = _collection.AsQueryable();

            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }

            return await Task.Run(() => query.Count(), token);
        }

        #endregion
    }
}