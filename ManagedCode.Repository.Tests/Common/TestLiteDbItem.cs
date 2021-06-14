using System;
using ManagedCode.Repository.LiteDB;

namespace ManagedCode.Repository.Tests.Common
{
    public class TestLiteDbItem : LiteDbItem<string>, IBaseItem<string>
    {
        public TestLiteDbItem()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string StringData { get; set; }
        public int IntData { get; set; }
        public float FloatData { get; set; }
        public DateTime DateTimeData { get; set; }
    }
}