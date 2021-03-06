﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using NUnit.Framework;

namespace ObservableComputations.Test
{
	[TestFixture]
	public class CollectionDispatchingTest
	{
		public class Item : INotifyPropertyChanged
		{
			public Item(int num, int num2, IDispatcher consuminingDispatcher, IDispatcher computingDispatcher)
			{
				_num = num;
				_num2 = num2;
				_numDispatching = new PropertyDispatching<Item, int>(() => Num, computingDispatcher, consuminingDispatcher);
				_num2Dispatching = new PropertyDispatching<Item, int>(() => Num2, consuminingDispatcher, computingDispatcher);
			}

			private int _num;
			public int Num
			{
				get => _num;
				set => updatePropertyValue(ref _num, value);
			}

			private int _num2;
			public int Num2
			{
				get => _num2;
				set => updatePropertyValue(ref _num2, value);
			}

			private PropertyDispatching<Item,int> _numDispatching;
			public PropertyDispatching<Item, int> NumDispatching => _numDispatching;

			private PropertyDispatching<Item,int> _num2Dispatching;
			public PropertyDispatching<Item, int> Num2Dispatching => _num2Dispatching;

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
		}

		[Test]
		public void TestCollectionDispatchingTest()
		{
			for (int j = 0; j < 1000000; j++)
			{
				Dispatcher consuminingDispatcher = new Dispatcher();
				Dispatcher computingDispatcher = new Dispatcher();

				var nums = new ObservableCollection<Item>();
				var filteredNums = nums.Filtering(i => i.Num % 3 == 0 || i.Num2Dispatching.Value % 5 == 0);
				var dispatchingfilteredNums = filteredNums.CollectionDispatching(consuminingDispatcher);
				bool stop = false;

				Random stopperRandom = new Random();
				Thread stopper = new Thread(() =>
				{
					Thread.Sleep(TimeSpan.FromSeconds(stopperRandom.Next(2, 20)));
					stop = true;
				});

				stopper.Start();

				ThreadStart numsChangerThreadStart = () =>
				{
					Random random =  new Random();
					while (!stop)
					{
						Thread.Sleep(random.Next(0, 3));

						int nextAction = random.Next(0, 20);
						if (nextAction > 3) nextAction = nextAction == 4 ? 4 : 0;
						NotifyCollectionChangedAction action = (NotifyCollectionChangedAction) nextAction;
						switch (action)
						{
							case NotifyCollectionChangedAction.Add:
								computingDispatcher.Invoke(() =>
								{
									int upperIndex = nums.Count > 0 ? nums.Count - 1 : 0;
									int index = random.Next(0, upperIndex);
									nums.Insert(index, new Item(random.Next(Int32.MinValue, int.MaxValue), random.Next(Int32.MinValue, int.MaxValue), consuminingDispatcher, computingDispatcher));
								});
								break;
							case NotifyCollectionChangedAction.Remove:
								computingDispatcher.Invoke(() =>
								{
									int upperIndex =  nums.Count - 1;
									if (upperIndex > 0)
									{
										int index = random.Next(0, upperIndex);
										nums.RemoveAt(index);
									}
								});
								break;
							case NotifyCollectionChangedAction.Replace:
								computingDispatcher.Invoke(() =>
								{
									int upperIndex =  nums.Count - 1;
									if (upperIndex > 0)
									{
										int index = random.Next(0, upperIndex);
										nums[index] = new Item(random.Next(Int32.MinValue, int.MaxValue), random.Next(Int32.MinValue, int.MaxValue), consuminingDispatcher, computingDispatcher);
									}

								});
								break;
							case NotifyCollectionChangedAction.Move:
								computingDispatcher.Invoke(() =>
								{
									int upperIndex =  nums.Count - 1;
									if (upperIndex > 0)
									{
										int indexFrom = random.Next(0, upperIndex);
										int indexTo = random.Next(0, upperIndex);
										nums.Move(indexFrom, indexTo);
									}
								});
								break;
							case NotifyCollectionChangedAction.Reset:
								computingDispatcher.Invoke(() => { nums.Clear(); });
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}

					}
				};


				int threadsCount = 10;   
				Thread[] numsChangerThreads = new Thread[threadsCount];
				for (int i = 0; i < threadsCount; i++)
				{
					numsChangerThreads[i] = new Thread(numsChangerThreadStart);
					numsChangerThreads[i].Start();
				}

				ThreadStart numValueChangerThreadStart = () =>
				{
					Random random =  new Random();
					while (!stop)
					{
						Thread.Sleep(random.Next(0, 3));

						consuminingDispatcher.Invoke(() =>
						{
							int dispatchingfilteredNumsCount = dispatchingfilteredNums.Count;
							if (dispatchingfilteredNumsCount > 0)
								dispatchingfilteredNums[random.Next(0, dispatchingfilteredNumsCount - 1)].NumDispatching.Value =
									random.Next(Int32.MinValue, int.MaxValue);
						});

					}
				};
 
				Thread[] numValueChangerThreads = new Thread[threadsCount];
				for (int i = 0; i < threadsCount; i++)
				{
					numValueChangerThreads[i] = new Thread(numValueChangerThreadStart);
					numValueChangerThreads[i].Start();
				}

				ThreadStart num2ValueChangerThreadStart = () =>
				{
					Random random =  new Random();
					while (!stop)
					{
						Thread.Sleep(random.Next(0, 3));

						consuminingDispatcher.Invoke(() =>
						{
							int dispatchingfilteredNumsCount = dispatchingfilteredNums.Count;
							if (dispatchingfilteredNumsCount > 0)
								dispatchingfilteredNums[random.Next(0, dispatchingfilteredNumsCount - 1)].Num2 =
									random.Next(Int32.MinValue, int.MaxValue);
						});

					}
				};
 
				Thread[] num2ValueChangerThreads = new Thread[threadsCount];
				for (int i = 0; i < threadsCount; i++)
				{
					num2ValueChangerThreads[i] = new Thread(num2ValueChangerThreadStart);
					num2ValueChangerThreads[i].Start();
				}


				//Thread consuminingDispatcherInvoker = new Thread(() =>
				//{
				//	Random random =  new Random();
				//	while (!stop)
				//	{
				//		Thread.Sleep(random.Next(0, 1000));
				//		consuminingDispatcher.Invoke(() =>
				//		{
				//			computingDispatcher.Invoke(() =>
				//			{


				//			});
				//		});
				//	}
				//});

				//consuminingDispatcherInvoker.Start();

				for (int i = 0; i < threadsCount; i++)
				{
					numsChangerThreads[i].Join();
				}

				for (int i = 0; i < threadsCount; i++)
				{
					numValueChangerThreads[i].Join();
				}

				for (int i = 0; i < threadsCount; i++)
				{
					num2ValueChangerThreads[i].Join();
				}

				//consuminingDispatcherInvoker.Join();

				consuminingDispatcher.Invoke(() => {});
				computingDispatcher.Invoke(() => {});
				consuminingDispatcher.Invoke(() => {});

				Assert.IsTrue(nums.Where(i => i.Num % 3 == 0 || i.Num2 % 5 == 0).SequenceEqual(dispatchingfilteredNums));
				Assert.IsTrue(nums.Where(i => i.NumDispatching.Value % 3 == 0 || i.Num2Dispatching.Value % 5 == 0).SequenceEqual(dispatchingfilteredNums));
				Debug.Print("!!!!!");
			}
		}
	}
}