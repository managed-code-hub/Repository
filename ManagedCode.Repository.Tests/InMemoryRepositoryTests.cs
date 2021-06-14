using ManagedCode.Repository.Core;
using ManagedCode.Repository.Tests.Common;

namespace ManagedCode.Repository.Tests
{
    public class InMemoryRepositoryTests : BaseRepositoryTests<int, InMemoryItem>
    {
        private static int _count;

        public InMemoryRepositoryTests() : base(new InMemoryRepository<int, InMemoryItem>())
        {
            Repository.InitializeAsync().Wait();
        }

        protected override int GenerateId()
        {
            _count++;
            return _count;
        }
    }
}