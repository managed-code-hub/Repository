using System;
using ManagedCode.Repository.AzureTable;

namespace ManagedCode.Repository.Tests.Common
{
    public class TestAzureTableItem : AzureTableItem, IBaseItem<TableId>
    {
        public string StringData { get; set; }
        public int IntData { get; set; }
        public float FloatData { get; set; }
        public DateTime DateTimeData { get; set; }
    }
}