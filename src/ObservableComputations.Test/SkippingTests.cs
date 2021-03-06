﻿using System.Collections.ObjectModel;
using NUnit.Framework;

namespace ObservableComputations.Test
{
	[TestFixture]
	public class SkippingTests
	{
		public class Item
		{
			public Item()
			{
				Num = LastNum;
				LastNum++;
			}

			public static int LastNum;
			public int Num;
		}

		[Test]
		public void Skipping_Initialization_01()
		{
			ObservableCollection<Item> items = new ObservableCollection<Item>();

			Skipping<Item> itemComputing = items.Skipping(0);
			itemComputing.ValidateConsistency();
		}


		[Test, Combinatorial]
		public void Skipping_Remove(
			[Range(0, 4, 1)] int count,
			[Range(0, 4, 1)] int index)
		{
			ObservableCollection<Item> items = new ObservableCollection<Item>(
				new[]
				{
					new Item(),
					new Item(),
					new Item(),
					new Item(),
					new Item()
				}
			);

			Skipping<Item> itemComputing = items.Skipping(count);
			itemComputing.ValidateConsistency();
			items.RemoveAt(index);
			itemComputing.ValidateConsistency();
		}

		[Test, Combinatorial]
		public void Skipping_Remove1(
			[Range(0, 4, 1)] int count)
		{
			ObservableCollection<Item> items = new ObservableCollection<Item>(
				new[]
				{
					new Item()
				}
			);

			Skipping<Item> itemComputing = items.Skipping(count);
			itemComputing.ValidateConsistency();
			items.RemoveAt(0);
			itemComputing.ValidateConsistency();
		}

		[Test, Combinatorial]
		public void Skipping_Insert(
			[Range(0, 4, 1)] int index,
			[Range(0, 4, 1)] int count)
		{
			ObservableCollection<Item> items = new ObservableCollection<Item>(
				new[]
				{
					new Item(),
					new Item(),
					new Item(),
					new Item(),
					new Item()
				}
			);

			Skipping<Item> itemComputing = items.Skipping( count);
			itemComputing.ValidateConsistency();
			items.Insert(index, new Item());
			itemComputing.ValidateConsistency();
		}

		[Test, Combinatorial]
		public void Skipping_Insert1(
			[Range(0, 4, 1)] int count)
		{
			ObservableCollection<Item> items = new ObservableCollection<Item>();

			Skipping<Item> itemComputing = items.Skipping(count);
			itemComputing.ValidateConsistency();
			items.Insert(0, new Item());
			itemComputing.ValidateConsistency();
		}

		[Test, Combinatorial]
		public void Skipping_Move(
			[Range(0, 4, 1)] int count,
			[Range(0, 4, 1)] int oldIndex,
			[Range(0, 4, 1)] int newIndex)
		{
			ObservableCollection<Item> items = new ObservableCollection<Item>(
				new[]
				{
					new Item(),
					new Item(),
					new Item(),
					new Item(),
					new Item()
				}
			);

			Skipping<Item> itemComputing = items.Skipping(count);
			itemComputing.ValidateConsistency();
			items.Move(oldIndex, newIndex);
			itemComputing.ValidateConsistency();
		}

		[Test, Combinatorial]
		public void Skipping_Set(
			[Range(0, 4, 1)] int count,
			[Range(0, 4, 1)] int index)
		{
			ObservableCollection<Item> items = new ObservableCollection<Item>(
				new[]
				{
					new Item(),
					new Item(),
					new Item(),
					new Item(),
					new Item()
				}
			);

			Skipping<Item> itemComputing = items.Skipping(count);
			itemComputing.ValidateConsistency();
			items[index] = new Item();
			itemComputing.ValidateConsistency();
		}		
	}
}