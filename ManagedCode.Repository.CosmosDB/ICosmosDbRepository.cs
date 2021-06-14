using ManagedCode.Repository.Core;

namespace ManagedCode.Repository.CosmosDB
{
    public interface ICosmosDbRepository<TItem> : IRepository<string, TItem> where TItem : CosmosDbItem, IItem<string>, new()
    {
    }
}