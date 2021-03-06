﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace ObservableComputations.Test
{
	[TestFixture]
	public class SummarizingTests
	{
		TextFileOutput _textFileOutputLog = new TextFileOutput(@"D:\Summarizing_Deep.log");
		TextFileOutput _textFileOutputTime = new TextFileOutput(@"D:\Summarizing_Deep_Time.log");

		[Test]
		public void Summarizing_Deep()
		{
			long counter = 0;
			Stopwatch stopwatch = Stopwatch.StartNew();
					
			test(new int[0]);

			for (int v1 = -2; v1 <= 2; v1++)
			{
				test(new []{v1});
				for (int v2 = -2; v2 <= 2; v2++)
				{
					test(new []{v1, v2});
					for (int v3 = -2; v3 <= 2; v3++)
					{
						test(new []{v1, v2, v3});
						for (int v4 = -2; v4 <= 2; v4++)
						{
							test(new []{v1, v2, v3, v4});
							for (int v5 = -2; v5 <= 2; v5++)
							{
								test(new[] {v1, v2, v3, v4, v5});
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
		}

		private void test(int[] values)
		{
			string testNum = string.Empty;
			int index = 0;
			int value = 0;
			int indexOld = 0;
			int indexNew = 0;

			ObservableCollection<int> items;
			Summarizing<int> summarizing;
			try
			{
				trace(testNum = "1", values, index, value, indexOld, indexNew);
				items = getObservableCollection(values);
				summarizing = items.Summarizing();
				validateSummarizingConsistency(summarizing, items);
				Assert.AreEqual(summarizing.Value, items.Sum());

				for (index = 0; index < values.Length; index++)
				{
					trace(testNum = "2", values, index, value, indexOld, indexNew);
					items = getObservableCollection(values);
					Summarizing<int> summarizing1 = items.Summarizing();
					items.RemoveAt(index);
					validateSummarizingConsistency(summarizing1, items);
					Assert.AreEqual(summarizing1.Value, items.Sum());
				}

				for (index = 0; index <= values.Length; index++)
				{
					for (value = 0; value <= values.Length; value++)
					{
						trace(testNum = "8", values, index, value, indexOld, indexNew);
						items = getObservableCollection(values);
						Summarizing<int> summarizing1 = items.Summarizing();
						items.Insert(index, value);
						validateSummarizingConsistency(summarizing1, items);
						Assert.AreEqual(summarizing1.Value, items.Sum());
					}
				}

				for (index = 0; index < values.Length; index++)
				{

					for (value = -1; value <= values.Length; value++)
					{
						trace(testNum = "3", values, index, value, indexOld, indexNew);
						items = getObservableCollection(values);
						Summarizing<int> summarizing2 = items.Summarizing();
						items[index] = value;
						validateSummarizingConsistency(summarizing2, items);
						Assert.AreEqual(summarizing2.Value, items.Sum());

					}
				}

				for (indexOld = 0; indexOld < values.Length; indexOld++)
				{
					for (indexNew = 0; indexNew < values.Length; indexNew++)
					{
						trace(testNum = "7", values, index, value, indexOld, indexNew);
						items = getObservableCollection(values);
						Summarizing<int> summarizing2 = items.Summarizing();
						items.Move(indexOld, indexNew);
						validateSummarizingConsistency(summarizing2, items);
						Assert.AreEqual(summarizing2.Value, items.Sum());
					}
				}
			}
			catch (Exception e)
			{
				string traceString = getTraceString(testNum, values, index, value, indexOld, indexNew);
				_textFileOutputLog.AppentLine(traceString);
				_textFileOutputLog.AppentLine(e.Message);
				_textFileOutputLog.AppentLine(e.StackTrace);
				throw new Exception(traceString, e);
			}

		}

		private void validateSummarizingConsistency(Summarizing<int> summarizing, ObservableCollection<int> items)
		{
			Assert.AreEqual(summarizing.Value, items.Sum());
		}

		private void trace(string testNum, int[] values, int index, int value, int indexOld, int indexNew)
		{
			string traceString = getTraceString(testNum, values, index, value, indexOld, indexNew);
			
			if (traceString == "#3. OrderNums=-1  index=0  value=-1   indexOld=0   indexNew=0")
			{
				Debugger.Break();
			}
		}

		private string getTraceString(string testNum, int[] values, int index, int value, int indexOld, int indexNew)
		{
			return string.Format(
				"#{5}. OrderNums={0}  index={1}  value={2}   indexOld={3}   indexNew={4}",
				string.Join(",", values),
				index,
				value,
				indexOld,
				indexNew,
				testNum);
		}

		private static ObservableCollection<int> getObservableCollection(int[] values)
		{
			return new ObservableCollection<int>(values);
		}



		//[Test, Combinatorial]
		//public void Summarizing_Change(
		//	[Range(-3, 2, 1)] int item1,
		//	[Range(-3, 2, 1)] int item2,
		//	[Range(-3, 2, 1)] int item3,
		//	[Range(-3, 2, 1)] int item4,
		//	[Range(-3, 2, 1)] int item5,
		//	[Range(0, 4, 1)] int index,
		//	[Range(-1, 5)] int newValue)
		//{
		//	ObservableCollection<int> items = new ObservableCollection<int>();
		//	if (item1 >= -2)
		//		items.Add(item1);
		//	if (item2 >= -2)
		//		items.Add(item2);
		//	if (item3 >= -2)
		//		items.Add(item3);
		//	if (item4 >= -2)
		//		items.Add(item4);
		//	if (item5 >= -2)
		//		items.Add(item5);

		//	if (index >= items.Count)
		//		return;

		//	Aggregating<int, int> summarizing = items.Summarizing();
		//	summarizing.ValidateConsistency();
		//	Assert.Equals(summarizing.Value, items.Sum());

		//	items[index] = newValue;

		//	summarizing.ValidateConsistency();
		//	Assert.Equals(summarizing.Value, items.Sum());

		//}

		//[Test, Combinatorial]
		//public void Summarizing_Remove(
		//	[Range(-3, 2, 1)] int item1,
		//	[Range(-3, 2, 1)] int item2,
		//	[Range(-3, 2, 1)] int item3,
		//	[Range(-3, 2, 1)] int item4,
		//	[Range(-3, 2, 1)] int item5,
		//	[Range(0, 4, 1)] int index)
		//{
		//	ObservableCollection<int> items = new ObservableCollection<int>();
		//	if (item1 >= -2) items.Add(item1);
		//	if (item2 >= -2) items.Add(item2);
		//	if (item3 >= -2) items.Add(item3);
		//	if (item4 >= -2) items.Add(item4);
		//	if (item5 >= -2) items.Add(item5);

		//	if (index >= items.Count) return;

		//	Aggregating<int, int> summarizing = items.Summarizing();
		//	summarizing.ValidateConsistency();
		//	Assert.Equals(summarizing.Value, items.Sum());

		//	items.RemoveAt(index);

		//	summarizing.ValidateConsistency();
		//	Assert.Equals(summarizing.Value, items.Sum());
		//}

		//[Test, Combinatorial]
		//public void Summarizing_Insert(
		//	[Range(-3, 2, 1)] int item1,
		//	[Range(-3, 2, 1)] int item2,
		//	[Range(-3, 2, 1)] int item3,
		//	[Range(-3, 2, 1)] int item4,
		//	[Range(-3, 2, 1)] int item5,
		//	[Range(0, 5, 1)] int index,
		//	[Range(-1, 5)] int newValue)
		//{
		//	ObservableCollection<int> items = new ObservableCollection<int>();
		//	if (item1 >= -2) items.Add(item1);
		//	if (item2 >= -2) items.Add(item2);
		//	if (item3 >= -2) items.Add(item3);
		//	if (item4 >= -2) items.Add(item4);
		//	if (item5 >= -2) items.Add(item5);

		//	if (index > items.Count) return;

		//	Aggregating<int, int> summarizing = items.Summarizing();

		//	summarizing.ValidateConsistency();
		//	Assert.Equals(summarizing.Value, items.Sum());

		//	items.Insert(index, newValue);

		//	summarizing.ValidateConsistency();
		//	Assert.Equals(summarizing.Value, items.Sum());
		//}

		//[Test, Combinatorial]
		//public void Summarizing_Move(
		//	[Range(-3, 2, 1)] int item1,
		//	[Range(-3, 2, 1)] int item2,
		//	[Range(-3, 2, 1)] int item3,
		//	[Range(-3, 2, 1)] int item4,
		//	[Range(-3, 2, 1)] int item5,
		//	[Range(0, 5, 1)] int oldIndex,
		//	[Range(0, 5, 1)] int newIndex)
		//{
		//	ObservableCollection<int> items = new ObservableCollection<int>();
		//	if (item1 >= -2) items.Add(item1);
		//	if (item2 >= -2) items.Add(item2);
		//	if (item3 >= -2) items.Add(item3);
		//	if (item4 >= -2) items.Add(item4);
		//	if (item5 >= -2) items.Add(item5);

		//	if (oldIndex >= items.Count || newIndex >= items.Count) return;

		//	Aggregating<int, int> summarizing = items.Summarizing();

		//	summarizing.ValidateConsistency();
		//	Assert.Equals(summarizing.Value, items.Sum());

		//	items.Move(oldIndex, newIndex);

		//	summarizing.ValidateConsistency();
		//	Assert.Equals(summarizing.Value, items.Sum());
		//}

	}
}