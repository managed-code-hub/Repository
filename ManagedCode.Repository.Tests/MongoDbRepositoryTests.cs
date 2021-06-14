using ManagedCode.Repository.MongoDB;
using ManagedCode.Repository.Tests.Common;
using MongoDB.Bson;

namespace ManagedCode.Repository.Tests
{
    public class MongoDbRepositoryTests : BaseRepositoryTests<ObjectId, TestMongoDbItem>
    {
        public const string ConnectionString =
            "mongodb://localhost:55000";

        public MongoDbRepositoryTests() : base(new MongoDbRepository<TestMongoDbItem>(new MongoDbRepositoryOptions
        {
            ConnectionString = ConnectionString,
            DataBaseName = "db"
        }))
        {
            Repository.InitializeAsync().Wait();
        }

        protected override ObjectId GenerateId()
        {
            return ObjectId.GenerateNewId();
        }
    }
}