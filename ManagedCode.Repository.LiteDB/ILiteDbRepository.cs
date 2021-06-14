using ManagedCode.Repository.Core;

namespace ManagedCode.Repository.LiteDB
{
    public interface ILiteDbRepository<TId, TItem> : IRepository<TId, TItem> where TItem : LiteDbItem<TId>, IItem<TId>, new()
    {
    }
}