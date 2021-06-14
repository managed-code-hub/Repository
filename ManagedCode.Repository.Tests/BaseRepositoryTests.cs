using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Repository.Core;
using ManagedCode.Repository.Core.Extensions;
using ManagedCode.Repository.Tests.Common;
using Xunit;

namespace ManagedCode.Repository.Tests
{
    public abstract class BaseRepositoryTests<TId, TItem> where TItem : IBaseItem<TId>, new()
    {
        protected readonly IRepository<TId, TItem> Repository;

        protected BaseRepositoryTests(IRepository<TId, TItem> repository)
        {
            Repository = repository;
        }

        protected abstract TId GenerateId();

        protected TItem CreateNewItem()
        {
            var rnd = new Random();
            return new TItem
            {
                Id = GenerateId(),
                StringData = Guid.NewGuid().ToString(),
                IntData = rnd.Next(0, 10),
                FloatData = Convert.ToSingle(rnd.NextDouble() + rnd.Next(0, 10)),
                DateTimeData = DateTime.Now
            };
        }

        protected TItem CreateNewItem(TId id)
        {
            var item = CreateNewItem();
            item.Id = id;
            return item;
        }

        [Fact]
        public async Task InitializeAsync()
        {
            await Repository.InitializeAsync();
            Repository.IsInitialized.Should().BeTrue();
            await Repository.InitializeAsync();
            Repository.IsInitialized.Should().BeTrue();
        }

        #region Insert

        [Fact]
        public virtual async Task InsertOneItem()
        {
            var id = GenerateId();
            var firstItem = CreateNewItem(id);
            var secondItem = CreateNewItem(id);

            var insertFirstItem = await Repository.InsertAsync(firstItem);
            var insertSecondItem = await Repository.InsertAsync(secondItem);

            insertFirstItem.Should().NotBeNull();
            insertSecondItem.Should().BeNull();
        }

        [Fact]
        public virtual async Task InsertListOfItems()
        {
            List<TItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Repository.InsertAsync(list);

            list.Count.Should().Be(100);
            items.Should().Be(100);
        }

        [Fact]
        public virtual async Task Insert99Items()
        {
            var id = GenerateId();

            await Repository.InsertAsync(CreateNewItem(id));

            List<TItem> list = new();

            list.Add(CreateNewItem(id));
            for (var i = 0; i < 99; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Repository.InsertAsync(list);

            list.Count.Should().Be(100);
            items.Should().Be(99);
        }

        [Fact]
        public virtual async Task InsertOrUpdateOneItem()
        {
            var id = GenerateId();
            for (var i = 0; i < 100; i++)
            {
                var insertOneItem = await Repository.InsertOrUpdateAsync(CreateNewItem(id));
                insertOneItem.Should().NotBeNull();
            }
        }

        [Fact]
        public virtual async Task InsertOrUpdateListOfItems()
        {
            List<TItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(CreateNewItem());
            }

            var itemsInsert = await Repository.InsertOrUpdateAsync(list);
            itemsInsert.Should().Be(100);

            foreach (var item in list)
            {
                item.DateTimeData = DateTime.Now.AddDays(-1);
            }
            
            var itemsUpdate = await Repository.InsertOrUpdateAsync(list);
            itemsUpdate.Should().Be(100);

            list.Count.Should().Be(100);
        }

        #endregion

        #region Update

        [Fact]
        public virtual async Task UpdateOneItem()
        {
            var id = GenerateId();

            var insertOneItem = await Repository.InsertAsync(CreateNewItem(id));
            var updateFirstItem = await Repository.UpdateAsync(CreateNewItem(id));
            var updateSecondItem = await Repository.UpdateAsync(CreateNewItem());

            insertOneItem.Should().NotBeNull();
            updateFirstItem.Should().NotBeNull();
            updateSecondItem.Should().BeNull();
        }

        [Fact]
        public virtual async Task UpdateListOfItems()
        {
            List<TItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Repository.InsertAsync(list);
            var updatedItems = await Repository.UpdateAsync(list.ToArray());

            items.Should().Be(100);
            updatedItems.Should().Be(100);
        }

        [Fact]
        public virtual async Task Update5Items()
        {
            List<TItem> list = new();

            var id = GenerateId();

            list.Add(CreateNewItem(id));
            for (var i = 0; i < 9; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Repository.InsertAsync(list);
            list.Clear();

            list.Add(CreateNewItem(id));
            for (var i = 0; i < 9; i++)
            {
                list.Add(CreateNewItem());
            }

            var updatedItems = await Repository.UpdateAsync(list);

            list.Count.Should().Be(10);
            items.Should().Be(10);
            updatedItems.Should().Be(1);
        }

        #endregion

        #region Delete

        [Fact]
        public virtual async Task DeleteOneItemById()
        {
            var item = CreateNewItem();
            await Repository.InsertAsync(item);
            var deleted = await Repository.DeleteAsync(item.Id);
            item.Should().NotBeNull();
            deleted.Should().BeTrue();
        }

        [Fact]
        public virtual async Task DeleteOneItem()
        {
            var item = CreateNewItem();
            await Repository.InsertAsync(item);
            var deleted = await Repository.DeleteAsync(item);
            item.Should().NotBeNull();
            deleted.Should().BeTrue();
        }

        [Fact]
        public virtual async Task DeleteListOfItems()
        {
            List<TItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Repository.InsertAsync(list);
            var deletedItems = await Repository.DeleteAsync(list);

            deletedItems.Should().Be(100);
            items.Should().Be(100);
        }

        [Fact]
        public virtual async Task DeleteListOfItemsById()
        {
            List<TItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Repository.InsertAsync(list);
            var ids = list.Select(s => s.Id);
            var deletedItems = await Repository.DeleteAsync(ids);

            deletedItems.Should().Be(100);
            items.Should().Be(100);
        }

        [Fact]
        public virtual async Task DeleteByQuery()
        {
            List<TItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(CreateNewItem());
            }

            await Repository.InsertOrUpdateAsync(list);

            var q1 = list[0].StringData;
            var q2 = list[1].StringData;
            var q3 = list[2].StringData;
            
            var equals = await Repository.DeleteAsync(w => w.StringData == q1);
            var or = await Repository.DeleteAsync(w => w.StringData == q2 || w.StringData == q3);

            equals.Should().Be(1);
            or.Should().Be(2);
        }

        [Fact]
        public virtual async Task DeleteAll()
        {
            List<TItem> list = new();

            for (var i = 0; i < 100; i++)
            {
                list.Add(CreateNewItem());
            }

            var items = await Repository.InsertAsync(list);
            var deletedItems = await Repository.DeleteAllAsync();
            var count = await Repository.CountAsync();

            deletedItems.Should().BeTrue();
            items.Should().Be(100);
            count.Should().Be(0);
        }

        #endregion

        #region Count

        [Fact]
        public virtual async Task Count()
        {
            await Repository.DeleteAllAsync();

            var insertOneItem = await Repository.InsertAsync(CreateNewItem());

            var count = await Repository.CountAsync();
            insertOneItem.Should().NotBeNull();
            count.Should().Be(1);
        }

        [Fact]
        public virtual async Task CountByQuery()
        {
            var count = 0;
            var guid = Guid.NewGuid().ToString();
            for (var i = 0; i < 100; i++)
            {
                var item = CreateNewItem();
                if (i % 2 == 0)
                {
                    item.StringData = guid;
                    count++;
                }

                await Repository.InsertAsync(item);
            }

            var deletedCount = await Repository.CountAsync(w => w.StringData == guid);
            deletedCount.Should().Be(count);
        }

        #endregion

        #region Get

        [Fact]
        public virtual async Task GetByWrongId()
        {
            var id1 = GenerateId();
            var id2 = GenerateId();

            var insertOneItem = await Repository.InsertAsync(CreateNewItem(id1));

            var item = await Repository.GetAsync(id2);
            insertOneItem.Should().NotBeNull();
            item.Should().BeNull();
        }

        [Fact]
        public virtual async Task GetById()
        {
            var id = GenerateId();
            var insertOneItem = await Repository.InsertAsync(CreateNewItem(id));

            var item = await Repository.GetAsync(id);
            insertOneItem.Should().NotBeNull();
            item.Should().NotBeNull();
        }

        [Fact]
        public virtual async Task GetByIdFirst()
        {
            var item1 = CreateNewItem();
            var item2 = CreateNewItem();

            await Repository.InsertAsync(item1);
            await Repository.InsertAsync(item2);

            var item = await Repository.GetAsync(w => w.StringData == item1.StringData || w.StringData == item2.StringData);
            item.Should().NotBeNull();
        }

        [Fact]
        public virtual async Task GetByQuery()
        {
            var item1 = CreateNewItem();
            var item2 = CreateNewItem();

            await Repository.InsertAsync(item1);
            await Repository.InsertAsync(item2);

            var item = await Repository.GetAsync(w => w.StringData == item1.StringData);
            item.Should().NotBeNull();
        }

        [Fact]
        public virtual async Task GetByWrongQuery()
        {
            await Repository.InsertAsync(CreateNewItem());
            await Repository.InsertAsync(CreateNewItem());

            var item = await Repository.GetAsync(w => w.StringData == "non existing value");
            item.Should().BeNull();
        }

        #endregion

        #region Find

        [Fact]
        public virtual async Task FindByCondition()
        {
            await Repository.DeleteAllAsync();
            
            var item1 = CreateNewItem();
            await Repository.InsertAsync(item1);
            for (var i = 0; i < 10; i++)
            {
                await Repository.InsertAsync(CreateNewItem());
            }

            var items = await Repository
                .FindAsync(Repository
                    .CreateCondition(x => x.StringData == item1.StringData, x => x.IntData == item1.IntData))
                .ToListAsync();

            items.Count.Should().Be(1);
        }

        [Fact]
        public virtual async Task Find()
        {
            await Repository.DeleteAllAsync();
            
            for (var i = 0; i < 100; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                await Repository.InsertAsync(item);
            }

            var items = await Repository.FindAsync(w => w.IntData >= 50).ToListAsync();
            items.Count.Should().Be(50);
        }

        [Fact]
        public virtual async Task FindTakeSkip()
        {
            await Repository.DeleteAllAsync();
            
            for (var i = 0; i < 100; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                await Repository.InsertAsync(item);
            }

            var items = await Repository.FindAsync(w => w.IntData > 10, 15, 10).ToListAsync();
            items.Count.Should().Be(15);
            items.First().IntData.Should().Be(21);
            items.Last().IntData.Should().Be(35);
        }

        [Fact]
        public virtual async Task FindTake()
        {
            await Repository.DeleteAllAsync();
            
            for (var i = 0; i < 100; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                await Repository.InsertAsync(item);
            }

            var items = await Repository.FindAsync(w => w.IntData >= 50, 10).ToListAsync();
            items.Count.Should().Be(10);
            items.First().IntData.Should().Be(50);
        }

        [Fact]
        public virtual async Task FindSkip()
        {
            await Repository.DeleteAllAsync();
            
            for (var i = 0; i < 100; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                await Repository.InsertAsync(item);
            }

            var items = await Repository.FindAsync(w => w.IntData >= 50, skip: 10).ToListAsync();
            items.Count.Should().Be(40);
            items.First().IntData.Should().Be(60);
        }

        [Fact]
        public virtual async Task FindOrder()
        {
            await Repository.DeleteAllAsync();
            
            for (var i = 0; i < 100; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                await Repository.InsertAsync(item);
            }

            var items = await Repository.FindAsync(w => w.IntData >= 50,
                    o => o.IntData, 10, 1)
                .ToListAsync();

            var itemsByDescending = await Repository.FindAsync(w => w.IntData >= 50,
                    o => o.IntData, Order.ByDescending, 10)
                .ToListAsync();

            items.Count.Should().Be(10);
            items[0].IntData.Should().Be(51);
            items[1].IntData.Should().Be(52);

            itemsByDescending.Count.Should().Be(10);
            itemsByDescending[0].IntData.Should().Be(99);
            itemsByDescending[1].IntData.Should().Be(98);
        }

        [Fact]
        public virtual async Task FindOrderThen()
        {
            await Repository.DeleteAllAsync();
            
            for (var i = 0; i < 100; i++)
            {
                var item = CreateNewItem();
                item.IntData = i;
                await Repository.InsertAsync(item);
            }

            var items = await Repository.FindAsync(w => w.IntData >= 50,
                    o => o.IntData, t => t.DateTimeData, 10, 1)
                .ToListAsync();

            var itemsBy = await Repository.FindAsync(w => w.IntData >= 50,
                    o => o.IntData, Order.ByDescending,  10)
                .ToListAsync();

            var itemsThenByDescending = await Repository.FindAsync(w => w.IntData >= 50,
                    o => o.IntData, Order.ByDescending, t => t.DateTimeData, Order.ByDescending, 10)
                .ToListAsync();

            items.Count.Should().Be(10);
            items[0].IntData.Should().Be(51);
            items[1].IntData.Should().Be(52);

            itemsBy.Count.Should().Be(10);
            itemsBy[0].IntData.Should().Be(99);
            itemsBy[1].IntData.Should().Be(98);

            itemsThenByDescending.Count.Should().Be(10);
            itemsThenByDescending[0].IntData.Should().Be(99);
            itemsThenByDescending[1].IntData.Should().Be(98);
        }

        #endregion
    }
}