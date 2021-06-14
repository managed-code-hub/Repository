using System;
using ManagedCode.Repository.MongoDB;
using MongoDB.Bson;

namespace ManagedCode.Repository.Tests.Common
{
    public class TestMongoDbItem : MongoDbItem, IBaseItem<ObjectId>
    {
        public string StringData { get; set; }
        public int IntData { get; set; }
        public float FloatData { get; set; }
        public DateTime DateTimeData { get; set; }
    }
}