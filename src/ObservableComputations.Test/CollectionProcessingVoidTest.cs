﻿using System.Collections.ObjectModel;
using NUnit.Framework;

namespace ObservableComputations.Test
{
	[TestFixture]
	public class CollectionProcessingVoidTest
	{
		public class Item
		{
			public bool ProcessedAsNew;
			public bool ProcessedAsOld;
		}

		private static CollectionProcessingVoid<Item> getCollectionProcessing(ObservableCollection<Item> items)
		{
			return items.CollectionProcessing(
				(item, current) =>
				{
					item.ProcessedAsNew = true;
				},
				(item, current) =>
				{
					item.ProcessedAsOld = true;
				});
		}


		[Test]
		public void CollectionProcessing_Initialization_01()
		{
			ObservableCollection<Item> items = new ObservableCollection<Item>();

			CollectionProcessingVoid<Item> collectionProcessing = getCollectionProcessing(items);
		}


		[Test, Combinatorial]
		public void CollectionProcessing_Remove(
			[Range(0, 4, 1)] int index)
		{
			Item[] sourceCollection = new[]
			{
				new Item(),
				new Item(),
				new Item(),
				new Item(),
				new Item()
			};

			ObservableCollection<Item> items = new ObservableCollection<Item>(
				sourceCollection);

			CollectionProcessingVoid<Item> collectionProcessing = getCollectionProcessing(items);
			items.RemoveAt(index);
			Assert.IsTrue(sourceCollection[index].ProcessedAsNew);
			Assert.IsTrue(sourceCollection[index].ProcessedAsOld);
		}

		[Test, Combinatorial]
		public void CollectionProcessing_Remove1()
		{
			Item item = new Item();
			ObservableCollection<Item> items = new ObservableCollection<Item>(
				new[]
				{
					item
				}
			);

			CollectionProcessingVoid<Item> collectionProcessing = getCollectionProcessing(items);
			items.RemoveAt(0);
			Assert.IsTrue(item.ProcessedAsNew);
			Assert.IsTrue(item.ProcessedAsOld);
		}

		[Test, Combinatorial]
		public void CollectionProcessing_Insert(
			[Range(0, 4, 1)] int index)
		{
			Item[] sourceCollection = new[]
			{
				new Item(),
				new Item(),
				new Item(),
				new Item(),
				new Item()
			};

			ObservableCollection<Item> items = new ObservableCollection<Item>(
				sourceCollection);

			CollectionProcessingVoid<Item> collectionProcessing = getCollectionProcessing(items);
			Item item = new Item();
			items.Insert(index, item);
			Assert.IsTrue(item.ProcessedAsNew);
			Assert.IsFalse(item.ProcessedAsOld);
		}

		[Test, Combinatorial]
		public void CollectionProcessing_Insert1(
			[Range(-1, 5)] int newValue)
		{
			ObservableCollection<Item> items = new ObservableCollection<Item>();

			CollectionProcessingVoid<Item> collectionProcessing = getCollectionProcessing(items);
			Item item = new Item();
			items.Insert(0, item);
			Assert.IsTrue(item.ProcessedAsNew);
			Assert.IsFalse(item.ProcessedAsOld);
		}

		[Test, Combinatorial]
		public void CollectionProcessing_Move(
			[Range(0, 4, 1)] int oldIndex,
			[Range(0, 4, 1)] int newIndex)
		{
			Item[] sourceCollection = new[]
			{
				new Item(),
				new Item(),
				new Item(),
				new Item(),
				new Item()
			};

			ObservableCollection<Item> items = new ObservableCollection<Item>(
				sourceCollection);

			CollectionProcessingVoid<Item> collectionProcessing = getCollectionProcessing(items);
			items.Move(oldIndex, newIndex);
			Assert.IsTrue(sourceCollection[oldIndex].ProcessedAsNew);
			Assert.IsFalse(sourceCollection[oldIndex].ProcessedAsOld);
		}

		[Test, Combinatorial]
		public void CollectionProcessing_Set(
			[Range(0, 4, 1)] int index)
		{
			Item[] sourceCollection = new[]
			{
				new Item(),
				new Item(),
				new Item(),
				new Item(),
				new Item()
			};

			ObservableCollection<Item> items = new ObservableCollection<Item>(
				sourceCollection);

			CollectionProcessingVoid<Item> collectionProcessing = getCollectionProcessing(items);
			items[index] = new Item();
			Assert.IsTrue(sourceCollection[index].ProcessedAsNew);
			Assert.IsTrue(sourceCollection[index].ProcessedAsOld);
			Assert.IsTrue(items[index].ProcessedAsNew);
			Assert.IsFalse(items[index].ProcessedAsOld);
		}	
	}
}