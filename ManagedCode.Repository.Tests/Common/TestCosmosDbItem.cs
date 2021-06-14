using System;
using ManagedCode.Repository.CosmosDB;

namespace ManagedCode.Repository.Tests.Common
{
    public class TestCosmosDbItem : CosmosDbItem, IBaseItem<string>
    {
        public string StringData { get; set; }
        public int IntData { get; set; }
        public float FloatData { get; set; }
        public DateTime DateTimeData { get; set; }
    }
}