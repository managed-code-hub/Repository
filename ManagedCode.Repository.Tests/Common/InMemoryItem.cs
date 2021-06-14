using System;

namespace ManagedCode.Repository.Tests.Common
{
    public class InMemoryItem : IBaseItem<int>
    {
        public int Id { get; set; }
        public string StringData { get; set; }
        public int IntData { get; set; }
        public float FloatData { get; set; }
        public DateTime DateTimeData { get; set; }
    }
}