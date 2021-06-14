using System;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Repository.CosmosDB;
using ManagedCode.Repository.Tests.Common;
using Xunit;

namespace ManagedCode.Repository.Tests
{
    public class CosmosDbRepositoryTests : BaseRepositoryTests<string, TestCosmosDbItem>
    {
        public const string ConnectionString =
            "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;";

        public CosmosDbRepositoryTests() : base(new CosmosDbRepository<TestCosmosDbItem>(
            new CosmosDbRepositoryOptions
            {
                ConnectionString = ConnectionString,
                DatabaseName = "database",
                CollectionName = "container",
            }))
        {
            Repository.InitializeAsync().Wait();
        }

        protected override string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        [Fact]
        public override async Task FindOrderThen()
        {
            Func<Task> act = () => base.FindOrderThen();

            act.Should().Throw<Exception>()
                .WithMessage("*The order by query does not have a corresponding composite index that it can be served from*");
        }
        
        [Fact]
        public override async Task Insert99Items()
        {
            Func<Task> act = () => base.Insert99Items();

            act.Should().Throw<Exception>()
                .WithMessage("*Resource with specified id or name already exists*");
        }
        
        [Fact]
        public override async Task UpdateOneItem()
        {
            Func<Task> act = () => base.UpdateOneItem();

            act.Should().Throw<Exception>()
                .WithMessage("*Resource Not Found*");
        }
        
        [Fact]
        public override async Task InsertOneItem()
        {
            Func<Task> act = () => base.InsertOneItem();

            act.Should().Throw<Exception>()
                .WithMessage("*Resource with specified id or name already exists*");
        }
        
        [Fact]
        public override async Task Update5Items()
        {
            Func<Task> act = () => base.Update5Items();

            act.Should().Throw<Exception>()
                .WithMessage("*Resource Not Found*");
        }
    }
}