using ManagedCode.Repository.Core;
using MongoDB.Bson;

namespace ManagedCode.Repository.MongoDB
{
    public interface IMongoDbRepository<TItem> : IRepository<ObjectId, TItem> where TItem : class, IItem<ObjectId>
    {
    }
}