﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace IBCode.ObservableComputations.Test
{
	[TestFixture]
	public class UnitingTests
	{
		public class Item : INotifyPropertyChanged
		{

			public Item()
			{
				Num = LastNum;
				LastNum++;
			}

			public static int LastNum;
			public int Num;

			#region INotifyPropertyChanged imlementation

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void onPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChangedEventHandler onPropertyChanged = PropertyChanged;
				if (onPropertyChanged != null) onPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}

			protected bool updatePropertyValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
			{
				if (EqualityComparer<T>.Default.Equals(field, value)) return false;
				field = value;
				this.onPropertyChanged(propertyName);
				return true;
			}

			#endregion

			public override string ToString()
			{
				return $"Num={Num}";
			}
		}

		TextFileOutput _textFileOutputLog = new TextFileOutput(@"D:\Projects\NevaPolimer\Uniting_Deep.log");
		TextFileOutput _textFileOutputTime = new TextFileOutput(@"D:\Projects\NevaPolimer\Uniting_Deep_Time.log");

		[Test, Combinatorial]
		public void Uniting_Deep()
		{
			long counter = 0;
			Stopwatch stopwatch = Stopwatch.StartNew();

			test(new int[0]);

			for (int v1 = -1; v1 <= 4; v1++)
			{
				test(new[] { v1 });
				for (int v2 = -1; v2 <= 4; v2++)
				{
					test(new[] { v1, v2 });
					for (int v3 = -1; v3 <= 4; v3++)
					{
						test(new[] { v1, v2, v3 });
						for (int v4 = -1; v4 <= 4; v4++)
						{
							test(new[] { v1, v2, v3, v4 });
							counter++;
							if (counter % 100 == 0)
							{
								_textFileOutputTime.AppentLine($"{stopwatch.Elapsed.TotalMinutes}: {counter}");
							}
						}
					}
				}
			}
		}

		private void test(int[] itemsCounts)
		{
			string testNum = string.Empty;
			int index = 0;
			int itemsCount = 0;
			int indexOld = 0;
			int indexNew = 0;
			int index1 = 0;
			ObservableCollection<ObservableCollection<Item>> items;
			Uniting<Item> uniting;
			try
			{
				trace(testNum = "1", itemsCounts, index, itemsCount, indexOld, indexNew);
				items = getObservableCollections(itemsCounts);
				uniting = items.Uniting();
				uniting.ValidateConsistency();

				for (index = 0; index < itemsCounts.Length; index++)
				{
					trace(testNum = "2", itemsCounts, index, itemsCount, indexOld, indexNew);
					items = getObservableCollections(itemsCounts);
					Uniting<Item> uniting1 = items.Uniting();
					items.RemoveAt(index);
					uniting1.ValidateConsistency();
				}

				for (index = 0; index <= itemsCounts.Length; index++)
				{
					for (itemsCount = 0; itemsCount <= itemsCounts.Length; itemsCount++)
					{
						trace(testNum = "11", itemsCounts, index, itemsCount, indexOld, indexNew);
						items = getObservableCollections(itemsCounts);
						Uniting<Item> uniting2 = items.Uniting();
						items.Insert(index, getObservableCollection(itemsCount));
						uniting2.ValidateConsistency();
					}
				}

				for (index = 0; index < itemsCounts.Length; index++)
				{
					trace(testNum = "6", itemsCounts, index, itemsCount, indexOld, indexNew);
					items = getObservableCollections(itemsCounts);
					Uniting<Item> uniting3 = items.Uniting();
					items[index] = new ObservableCollection<Item>();
					uniting3.ValidateConsistency();

					for (itemsCount = 0; itemsCount <= itemsCounts.Length; itemsCount++)
					{
						trace(testNum = "3", itemsCounts, index, itemsCount, indexOld, indexNew);
						items = getObservableCollections(itemsCounts);
						Uniting<Item> uniting2 = items.Uniting();
						items[index] = getObservableCollection(itemsCount);
						uniting2.ValidateConsistency();

					}
				}

				for (index = 0; index < itemsCounts.Length; index++)
				{
					trace(testNum = "4", itemsCounts, index, itemsCount, indexOld, indexNew);
					items = getObservableCollections(itemsCounts);
					Uniting<Item> uniting1 = items.Uniting();
					items[index] = null;
					uniting1.ValidateConsistency();

					for (itemsCount = 0; itemsCount <= itemsCounts.Length; itemsCount++)
					{
						trace(testNum = "5", itemsCounts, index, itemsCount, indexOld, indexNew);
						items = getObservableCollections(itemsCounts);
						Uniting<Item> uniting2 = items.Uniting();
						items[index] = getObservableCollection(itemsCount);
						uniting2.ValidateConsistency();

					}
				}

				for (indexOld = 0; indexOld < itemsCounts.Length; indexOld++)
				{
					for (indexNew = 0; indexNew < itemsCounts.Length; indexNew++)
					{
						trace(testNum = "6", itemsCounts, index, itemsCount, indexOld, indexNew);
						items = getObservableCollections(itemsCounts);
						Uniting<Item> uniting2 = items.Uniting();
						items.Move(indexOld, indexNew);
						uniting2.ValidateConsistency();
					}
				}



				for (index1 = 0; index1 < itemsCounts.Length; index1++)
				{
					int itemsCount1 = itemsCounts[index1];
					for (index = 0; index < itemsCount1; index++)
					{
						trace(testNum = "7", itemsCounts, index, itemsCount, indexOld, indexNew);
						items = getObservableCollections(itemsCounts);
						Uniting<Item> uniting1 = items.Uniting();
						items[index1].RemoveAt(index);
						uniting1.ValidateConsistency();
					}

					for (index = 0; index <= itemsCount1; index++)
					{
						trace(testNum = "12", itemsCounts, index, itemsCount, indexOld, indexNew);
						items = getObservableCollections(itemsCounts);
						Uniting<Item> uniting1 = items.Uniting();
						items[index1].Insert(index, new Item());
						uniting1.ValidateConsistency();
					}

					for (index = 0; index < itemsCount1; index++)
					{
						trace(testNum = "4", itemsCounts, index, itemsCount, indexOld, indexNew);
						items = getObservableCollections(itemsCounts);
						Uniting<Item> uniting3 = items.Uniting();
						items[index1][index] = null;
						uniting3.ValidateConsistency();

						trace(testNum = "9", itemsCounts, index, itemsCount, indexOld, indexNew);
						items = getObservableCollections(itemsCounts);
						Uniting<Item> uniting2 = items.Uniting();
						items[index1][index] = new Item();
						uniting2.ValidateConsistency();
					}

					for (indexOld = 0; indexOld < itemsCount1; indexOld++)
					{
						for (indexNew = 0; indexNew < itemsCount1; indexNew++)
						{
							trace(testNum = "10", itemsCounts, index, itemsCount, indexOld, indexNew);
							items = getObservableCollections(itemsCounts);
							Uniting<Item> uniting2 = items.Uniting();
							items[index1].Move(indexOld, indexNew);
							uniting2.ValidateConsistency();
						}
					}
				}
			}
			catch (Exception e)
			{
				string traceString = getTraceString( 
					testNum,
					itemsCounts,
					index,
					itemsCount, 
					indexOld,
					indexNew, 
					index1);
				_textFileOutputLog.AppentLine(traceString);
				_textFileOutputLog.AppentLine(e.Message);
				_textFileOutputLog.AppentLine(e.StackTrace);
				throw new Exception(traceString, e);
			}

		}

		private void trace(string num, int[] itemsCounts, int index, int itemsCount, int indexOld,
			int indexNew, int index1 = 0)
		{
			string traceString = getTraceString(num, itemsCounts, index, itemsCount, indexOld, indexNew, index1);

			if (traceString == "#6. ItemsCounts=-1,-1,1,-1  index=4  itemsCount=5   indexOld=2   indexNew=3, index1=0")
			{
				
			}
		}

		private static string getTraceString(string num, int[] itemsCounts, int index, int itemsCount, int indexOld, int indexNew, int index1 = 0)
		{
			return string.Format(
				"#{6}. ItemsCounts={0}  index={1}  itemsCount={2}   indexOld={3}   indexNew={4}, index1={5}",
				string.Join(",", itemsCounts),
				index,
				itemsCount,
				indexOld,
				indexNew,
				index1,
				num);
		}


		private static ObservableCollection<ObservableCollection<Item>> getObservableCollections(int[] itemsCounts)
		{
			return new ObservableCollection<ObservableCollection<Item>>(itemsCounts.Select(itemsCount => getObservableCollection(itemsCount)));
		}

		private static ObservableCollection<Item> getObservableCollection(int itemsCount)
		{
			return itemsCount >= 0 
				? new ObservableCollection<Item>(Enumerable.Range(1, itemsCount).Select(i => new Item()))
				: null;
		}
	}
}
