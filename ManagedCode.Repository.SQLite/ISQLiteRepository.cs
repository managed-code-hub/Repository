using ManagedCode.Repository.Core;

namespace ManagedCode.Repository.SQLite
{
    public interface ISQLiteRepository<TItem> : IRepository<int, TItem> where TItem : SQLiteItem, new()
    {
    }
}