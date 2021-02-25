using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Repository.Core;
using ManagedCode.Repository.CosmosDB;
using Xunit;
using CosmosDbItem = ManagedCode.Repository.Tests.Common.CosmosDbItem;

namespace ManagedCode.Repository.Tests
{
    public class CosmosDbRepositoryTests
    {
        public const string ConnecntionString =
            "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;";

        private readonly CosmosDB.ICosmosDbRepository<CosmosDbItem> _repository = new CosmosDbRepository<CosmosDbItem>(null, new CosmosDbRepositoryOptions
        {
            ConnectionString = ConnecntionString
        });

        public CosmosDbRepositoryTests()
        {
            _repository.InitializeAsync().Wait();
            //_repository.DeleteAllAsync().Wait();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task InitializeAsync()
        {
            await _repository.InitializeAsync();
            _repository.IsInitialized.Should().BeTrue();
            await _repository.InitializeAsync();
            _repository.IsInitialized.Should().BeTrue();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task NotInitializedAsync()
        {
            var localRepository = new CosmosDbRepository<CosmosDbItem>(null, new CosmosDbRepositoryOptions
            {
                ConnectionString = ConnecntionString
            });

            localRepository.IsInitialized.Should().BeFalse();

            var item = await localRepository.InsertAsync(new CosmosDbItem());

            item.Should().NotBeNull();
        }

        #region Find

        [Fact(Skip = "Emulator issue")]
        public async Task Find()
        {
            var list = new List<CosmosDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "Find",
                    Id = "Find" + i,
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items = await _repository.FindAsync(w => w.PartKey == "Find" && w.IntData >= 50).ToListAsync();
            items.Count.Should().Be(50);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task FindTakeSkip()
        {
            var list = new List<CosmosDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "FindTakeSkip",
                    Id = "FindTakeSkip" + i,
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items1 = await _repository.FindAsync(w => w.PartKey == "FindTakeSkip" && w.IntData > 0, 15).ToListAsync();
            var items2 = await _repository.FindAsync(w => w.PartKey == "FindTakeSkip" && w.IntData > 0, 15, 10).ToListAsync();
            items1.Count.Should().Be(15);
            items2.Count.Should().Be(15);
            items1[10].Data.Should().BeEquivalentTo(items2[0].Data);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task FindTake()
        {
            var list = new List<CosmosDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "FindTake",
                    Id = "FindTake" + i,
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items1 = await _repository.FindAsync(w => w.PartKey == "FindTake" && w.IntData >= 50, 10).ToListAsync();
            var items2 = await _repository.FindAsync(w => w.PartKey == "FindTake" && w.IntData >= 50, 15).ToListAsync();
            items1.Count.Should().Be(10);
            items2.Count.Should().Be(15);
            items1[0].Data.Should().Be(items2[0].Data);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task FindSkip()
        {
            var list = new List<CosmosDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "FindSkip",
                    Id = "FindSkip" + i,
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items1 = await _repository.FindAsync(w => w.PartKey == "FindSkip" && w.IntData >= 50, skip: 10).ToListAsync();
            var items2 = await _repository.FindAsync(w => w.PartKey == "FindSkip" && w.IntData >= 50, skip: 11).ToListAsync();
            items1.Count.Should().Be(40);
            items2.Count.Should().Be(39);
            items1[1].IntData.Should().Be(items2[0].IntData);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task FindOrder()
        {
            var list = new List<CosmosDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "FindOrder",
                    Id = "FindOrder" + i,
                    IntData = i,
                    Data = $"item{i}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items = await _repository.FindAsync(w => w.PartKey == "FindOrder" && w.IntData > 9,
                    o => o.Id, 10, 1)
                .ToListAsync();

            var itemsByDescending = await _repository.FindAsync(w => w.PartKey == "FindOrder" && w.IntData > 10,
                    o => o.Id, Order.ByDescending, 10)
                .ToListAsync();

            items.Count.Should().Be(10);
            items[0].IntData.Should().Be(11);
            items[1].IntData.Should().Be(12);

            itemsByDescending.Count.Should().Be(10);
            itemsByDescending[0].IntData.Should().Be(99);
            itemsByDescending[1].IntData.Should().Be(98);
        }

        [Fact(Skip = "The order by query does not have a corresponding composite index that it can be served from. CompositeIndexes required.")]
        public async Task FindOrderThen()
        {
            var list = new List<CosmosDbItem>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "FindOrderThen",
                    Id = "FindOrderThen" + i,
                    IntData = i,
                    Data = $"item{i % 2}"
                });
            }

            await _repository.InsertOrUpdateAsync(list);

            var items = await _repository.FindAsync(w => w.PartKey == "FindOrderThen" && w.IntData >= 9,
                    o => o.Data, t => t.IntData, 10, 1)
                .ToListAsync();

            var itemsBy = await _repository.FindAsync(w => w.PartKey == "FindOrderThen" && w.IntData >= 10,
                    o => o.Data, Order.ByDescending, t => t.IntData, 10)
                .ToListAsync();

            var itemsThenByDescending = await _repository.FindAsync(w => w.PartKey == "FindOrderThen" && w.IntData >= 10,
                    o => o.Data, Order.ByDescending, t => t.IntData, Order.ByDescending, 10)
                .ToListAsync();

            items.Count.Should().Be(10);
            items[0].IntData.Should().Be(11);
            items[1].IntData.Should().Be(12);

            itemsBy.Count.Should().Be(10);
            itemsBy[0].IntData.Should().Be(10);
            itemsBy[1].IntData.Should().Be(11);

            itemsThenByDescending.Count.Should().Be(10);
            itemsThenByDescending[0].IntData.Should().Be(10);
            itemsThenByDescending[1].IntData.Should().Be(11);
        }

        #endregion

        #region Insert

        [Fact(Skip = "Emulator issue")]
        public async Task InsertOneItem()
        {
            var insertFirstItem = await _repository.InsertAsync(new CosmosDbItem
            {
                Id = "InsertOneItem",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var insertSecondItem = await _repository.InsertAsync(new CosmosDbItem
            {
                Id = "InsertOneItem",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            insertFirstItem.Should().NotBeNull();
            insertSecondItem.Should().BeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task InsertListOfItems()
        {
            List<CosmosDbItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = $"InsertListOfItems{i % 2}",
                    Id = "InsertListOfItems" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            items.Should().Be(150);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task Insert100Items()
        {
            await _repository.InsertAsync(new CosmosDbItem
            {
                RowKey = "Insert100Items",
                Id = "Insert100Items140",
                Data = Guid.NewGuid().ToString()
            });

            List<CosmosDbItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new CosmosDbItem
                {
                    RowKey = "Insert100Items",
                    Id = "Insert100Items" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            items.Should().Be(149);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task InsertOrUpdateOneItem()
        {
            var insertOneItem = await _repository.InsertOrUpdateAsync(new CosmosDbItem
            {
                PartKey = "InsertOrUpdateOneItem",
                Id = "InsertOrUpdateOneItem",
                Data = Guid.NewGuid().ToString()
            });

            var insertTwoItem = await _repository.InsertOrUpdateAsync(new CosmosDbItem
            {
                PartKey = "InsertOrUpdateOneItem",
                Id = "InsertOrUpdateOneItem",
                Data = Guid.NewGuid().ToString()
            });

            insertOneItem.Should().BeTrue();
            insertTwoItem.Should().BeTrue();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task InsertOrUpdateListOfItems()
        {
            List<CosmosDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = $"InsertOrUpdateListOfItems{i % 2}",
                    Id = "InsertOrUpdateListOfItems" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var itemsFirst = await _repository.InsertOrUpdateAsync(list);
            var itemsSecond = await _repository.InsertOrUpdateAsync(list);

            itemsFirst.Should().Be(100);
            itemsSecond.Should().Be(100);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task InsertOrUpdate100Items()
        {
            await _repository.InsertOrUpdateAsync(new CosmosDbItem
            {
                PartKey = "InsertOrUpdate100Items",
                Id = "InsertOrUpdate100Items1",
                Data = Guid.NewGuid().ToString()
            });

            List<CosmosDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "InsertOrUpdate100Items",
                    Id = "InsertOrUpdate100Items1" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var itemsFirst = await _repository.InsertOrUpdateAsync(list);
            var itemsSecond = await _repository.InsertOrUpdateAsync(list);

            itemsFirst.Should().Be(100);
            itemsSecond.Should().Be(100);
        }

        #endregion

        #region Update

        [Fact(Skip = "Emulator issue")]
        public async Task UpdateOneItem()
        {
            var insertOneItem = await _repository.InsertAsync(new CosmosDbItem
            {
                PartKey = "UpdateOneItem",
                Id = "rk",
                Data = "test"
            });

            var updateFirstItem = await _repository.UpdateAsync(new CosmosDbItem
            {
                PartKey = "UpdateOneItem",
                Id = "rk",
                Data = "test-test"
            });

            var updateSecondItem = await _repository.UpdateAsync(new CosmosDbItem
            {
                PartKey = "UpdateOneItem",
                Id = "rk-rk",
                Data = "test"
            });

            insertOneItem.Should().NotBeNull();
            updateFirstItem.Should().BeTrue();
            updateSecondItem.Should().BeFalse();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task UpdateListOfItems()
        {
            List<CosmosDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "UpdateListOfItems",
                    Id = "UpdateListOfItems" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);

            list.Clear();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "UpdateListOfItems",
                    Id = "UpdateListOfItems" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var updatedItems = await _repository.UpdateAsync(list);

            items.Should().Be(100);
            updatedItems.Should().Be(100);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task Update5Items()
        {
            List<CosmosDbItem> list = new();

            for (var i = 0; i < 5; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "Update5Items",
                    Id = "Update5Items" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);
            list.Clear();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "Update5Items",
                    Id = "Update5Items" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var updatedItems = await _repository.UpdateAsync(list);

            items.Should().Be(5);
            updatedItems.Should().Be(0);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task Update10Items()
        {
            List<CosmosDbItem> list = new();

            for (var i = 0; i < 10; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "Update10Items",
                    Id = "Update10Items" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertAsync(list);
            list.Clear();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "Update10Items",
                    Id = "Update10Items" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var insertedItems = await _repository.InsertAsync(list);

            items.Should().Be(10);
            insertedItems.Should().Be(90);
        }

        #endregion

        #region Delete

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteOneItemById()
        {
            var insertOneItem = await _repository.InsertOrUpdateAsync(new CosmosDbItem
            {
                Id = "DeleteOneItemById",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var deleteOneItem = await _repository.DeleteAsync("DeleteOneItemById");
            insertOneItem.Should().BeTrue();
            deleteOneItem.Should().BeTrue();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteOneItem()
        {
            var item = new CosmosDbItem
            {
                PartKey = "DeleteOneItem",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            };

            var insertOneItem = await _repository.InsertOrUpdateAsync(item);

            var deleteOneTimer = await _repository.DeleteAsync(item);
            insertOneItem.Should().BeTrue();
            deleteOneTimer.Should().BeTrue();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteListOfItems()
        {
            List<CosmosDbItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "DeleteListOfItems",
                    Id = "DeleteListOfItems" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertOrUpdateAsync(list);
            var deletedItems = await _repository.DeleteAsync(list);

            deletedItems.Should().Be(150);
            items.Should().Be(150);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteListOfItemsById()
        {
            List<CosmosDbItem> list = new();

            for (var i = 0; i < 150; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "DeleteListOfItemsById",
                    Id = "DeleteListOfItemsById" + i,
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertOrUpdateAsync(list);
            var deletedItems = await _repository.DeleteAsync(list.Select(s => s.Id));

            items.Should().Be(150);
            deletedItems.Should().Be(150);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteByQuery()
        {
            List<CosmosDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    PartKey = "DeleteByQuery",
                    Id = "DeleteByQuery" + i,
                    IntData = i,
                    Data = i >= 50 ? i.ToString() : string.Empty
                });
            }

            await _repository.InsertOrUpdateAsync(list);
            var items = await _repository.DeleteAsync(w => w.PartKey == "DeleteByQuery" && w.IntData >= 50);
            items.Should().Be(50);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task DeleteAll()
        {
            List<CosmosDbItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(new CosmosDbItem
                {
                    Id = "DeleteAll" + i,
                    PartKey = "DeleteAll",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var items = await _repository.InsertOrUpdateAsync(list);
            var deletedItems = await _repository.DeleteAllAsync();
            var count = await _repository.CountAsync();

            deletedItems.Should().BeTrue();
            items.Should().Be(100);
            count.Should().Be(0);
        }

        #endregion

        #region Get

        [Fact(Skip = "Emulator issue")]
        public async Task GetByWrongId()
        {
            var insertOneItem = await _repository.InsertAsync(new CosmosDbItem
            {
                PartKey = "GetByWrongId",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var item = await _repository.GetAsync("GetByWrongId");
            insertOneItem.Should().NotBeNull();
            item.Should().BeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task GetById()
        {
            var items = new List<CosmosDbItem>();
            for (var i = 0; i < 100; i++)
            {
                items.Add(new CosmosDbItem
                {
                    Id = "GetById" + i,
                    PartKey = "GetById",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var insertOneItem = await _repository.InsertOrUpdateAsync(items);

            var item = await _repository.GetAsync("GetById10");
            insertOneItem.Should().Be(100);
            item.Should().NotBeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task GetByQuery()
        {
            var items = new List<CosmosDbItem>();
            for (var i = 0; i < 100; i++)
            {
                items.Add(new CosmosDbItem
                {
                    PartKey = "GetByQuery",
                    Id = "GetByQuery" + i,
                    RowKey = "4",
                    Data = $"item{i}"
                });
            }

            var addedItems = await _repository.InsertOrUpdateAsync(items);

            var item = await _repository.GetAsync(w => w.Data == "item4" && w.RowKey == "4" && w.PartKey == "GetByQuery");
            addedItems.Should().Be(100);
            item.Should().NotBeNull();
        }

        [Fact(Skip = "Emulator issue")]
        public async Task GetByWrongQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new CosmosDbItem
                {
                    PartKey = "GetByWrongQuery",
                    RowKey = i.ToString(),
                    Data = Guid.NewGuid().ToString()
                });
            }

            var item = await _repository.GetAsync(w => w.Data == "some");
            item.Should().BeNull();
        }

        #endregion

        #region Count

        [Fact(Skip = "Emulator issue")]
        public async Task Count()
        {
            var insertOneItem = await _repository.InsertOrUpdateAsync(new CosmosDbItem
            {
                PartKey = "Count",
                RowKey = "rk",
                Data = Guid.NewGuid().ToString()
            });

            var count = await _repository.CountAsync();
            insertOneItem.Should().BeTrue();
            count.Should().BeGreaterOrEqualTo(1);
        }

        [Fact(Skip = "Emulator issue")]
        public async Task CountByQuery()
        {
            for (var i = 0; i < 100; i++)
            {
                await _repository.InsertAsync(new CosmosDbItem
                {
                    PartKey = "CountByQuery",
                    Id = "CountByQuery" + i,
                    IntData = i
                });
            }

            var count = await _repository.CountAsync(w => w.PartKey == "CountByQuery" && w.IntData == 4);
            count.Should().Be(1);
        }

        #endregion
    }
}