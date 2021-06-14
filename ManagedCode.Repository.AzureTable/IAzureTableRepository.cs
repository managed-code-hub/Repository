using ManagedCode.Repository.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Repository.AzureTable
{
    public interface IAzureTableRepository<T> : IRepository<TableId, T>
        where T : class, IItem<TableId>, ITableEntity, new()
    {
    }
}