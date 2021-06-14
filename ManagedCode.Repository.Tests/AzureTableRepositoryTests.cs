using System;
using ManagedCode.Repository.AzureTable;
using ManagedCode.Repository.Tests.Common;

namespace ManagedCode.Repository.Tests
{
    public class AzureTableRepositoryTests : BaseRepositoryTests<TableId, TestAzureTableItem>
    {
        public const string ConnectionString =
            "DefaultEndpointsProtocol=http;AccountName=localhost;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;TableEndpoint=http://localhost:8902/;";

        public AzureTableRepositoryTests() : base(new AzureTableRepository<TestAzureTableItem>(
            new AzureTableRepositoryOptions
            {
                ConnectionString = ConnectionString
            }))
        {
            Repository.InitializeAsync().Wait();
        }

        protected override TableId GenerateId()
        {
            return new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }
    }
}