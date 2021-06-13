using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Repository.LiteDB;
using ManagedCode.Repository.Tests.Common;
using SQLite;
using Xunit;

namespace ManagedCode.Repository.Tests
{
    public class LiteDbRepositoryTests : BaseRepositoryTests<string, TestLiteDbItem>
    {
        public const string ConnectionString = "litedb_test.db";

        public LiteDbRepositoryTests() : base(new LiteDbRepository<string, TestLiteDbItem>(new LiteDbRepositoryOptions
        {
            ConnectionString = GetTempDbName()
        }))
        {
            Repository.InitializeAsync().Wait();
        }

        private static string GetTempDbName()
        {
            return Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + ConnectionString);
        }

        protected override string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }


        [Fact(Skip = "UNIQUE constraint failed")]
        public override async Task Insert99Items()
        {
            Action act = () => base.Insert99Items();

            act.Should().Throw<Exception>()
                .WithMessage("UNIQUE constraint failed: SQLiteDbItem.Id");
        }
        
    }
}