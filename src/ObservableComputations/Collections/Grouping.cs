﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using ObservableComputations.ExtentionMethods;

namespace ObservableComputations
{
	// TODO реализовать IDictionary в Grouping
	// TODO если TKey это INotifyCollectionChanged реагировать на CollectionChanged
	// TODO Сделать GettingDictionary : Dictionary 
	public class Grouping<TSourceItem, TKey> : CollectionComputing<Group<TSourceItem, TKey>>, IHasSourceCollections
	{
		public IReadScalar<INotifyCollectionChanged> SourceScalar => _sourceScalar;

		public Expression<Func<TSourceItem, TKey>> KeySelectorExpression => _keySelectorExpressionOriginal;

		public INotifyCollectionChanged Source => _source;

		public IReadScalar<IEqualityComparer<TKey>> EqualityComparerScalar => _equalityComparerScalar;

		public IEqualityComparer<TKey> EqualityComparer => _equalityComparer;

		// ReSharper disable once MemberCanBePrivate.Global
		public Func<TSourceItem, TKey> KeySelectorFunc => _keySelectorFunc;

		public ReadOnlyCollection<INotifyCollectionChanged> SourceCollections => new ReadOnlyCollection<INotifyCollectionChanged>(new []{Source});
		public ReadOnlyCollection<IReadScalar<INotifyCollectionChanged>> SourceCollectionScalars => new ReadOnlyCollection<IReadScalar<INotifyCollectionChanged>>(new []{SourceScalar});

		public Action<Group<TSourceItem, TKey>, int, TSourceItem> InsertItemIntoGroupAction
		{
			get => _insertItemIntoGroupAction;
			set
			{
				if (_insertItemIntoGroupAction != value)
				{
					checkLockModifyGroupChangeAction(CollectionChangeAction.InsertItem);

					_insertItemIntoGroupAction = value;
					OnPropertyChanged(Utils.InsertItemIntoGroupActionPropertyChangedEventArgs);
				}

			}
		}

		public Action<Group<TSourceItem, TKey>, int> RemoveItemFromGroupAction
		{
			get => _removeItemFromGroupAction;
			set
			{
				if (_removeItemFromGroupAction != value)
				{
					checkLockModifyGroupChangeAction(CollectionChangeAction.RemoveItem);

					_removeItemFromGroupAction = value;
					OnPropertyChanged(Utils.RemoveItemFromGroupActionPropertyChangedEventArgs);
				}
			}
		}

		public Action<Group<TSourceItem, TKey>, int, int> MoveItemInGroupAction
		{
			get => _moveItemInGroupAction;
			set
			{
				if (_moveItemInGroupAction != value)
				{
					checkLockModifyGroupChangeAction(CollectionChangeAction.MoveItem);

					_moveItemInGroupAction = value;
					OnPropertyChanged(Utils.MoveItemInGroupActionPropertyChangedEventArgs);
				}
			}
		}

		public Action<Group<TSourceItem, TKey>> ClearGroupItemsAction
		{
			get => _clearGroupItemsAction;
			set
			{
				if (_clearGroupItemsAction != value)
				{
					checkLockModifyGroupChangeAction(CollectionChangeAction.ClearItems);

					_clearGroupItemsAction = value;
					OnPropertyChanged(Utils.ClearGroupItemsActionPropertyChangedEventArgs);
				}
			}
		}

		public Action<Group<TSourceItem, TKey>, int, TSourceItem> SetGroupItemAction
		{
			get => _setGroupItemAction;
			set
			{
				if (_setGroupItemAction != value)
				{
					checkLockModifyGroupChangeAction(CollectionChangeAction.SetItem);

					_setGroupItemAction = value;
					OnPropertyChanged(Utils.SetGroupItemActionPropertyChangedEventArgs);
				}
			}
		}


		Dictionary<CollectionChangeAction, object> _lockModifyGroupChangeActionsKeys;
		private Dictionary<CollectionChangeAction, object> lockModifyGroupChangeActionsKeys => _lockModifyGroupChangeActionsKeys = 
			_lockModifyGroupChangeActionsKeys ?? new Dictionary<CollectionChangeAction, object>();

		public void LockModifyGroupChangeAction(CollectionChangeAction collectionChangeAction, object key)
		{
			if (key == null) throw new ArgumentNullException("key");

			if (!lockModifyGroupChangeActionsKeys.ContainsKey(collectionChangeAction))
				lockModifyGroupChangeActionsKeys[collectionChangeAction] = key;
			else
				throw new ObservableComputationsException(this,
					$"Modifying of '{collectionChangeAction.ToString()}' group change action is already locked. Unlock first.");
		}

		public void UnlockModifyGroupChangeAction(CollectionChangeAction collectionChangeAction, object key)
		{
			if (key == null) throw new ArgumentNullException("key");

			if (!lockModifyGroupChangeActionsKeys.ContainsKey(collectionChangeAction))
				throw new ObservableComputationsException(this,
					"Modifying of '{collectionChangeAction.ToString()}' group change action is not locked. Lock first.");

			if (ReferenceEquals(lockModifyGroupChangeActionsKeys[collectionChangeAction], key))
				lockModifyGroupChangeActionsKeys.Remove(collectionChangeAction);
			else
				throw new ObservableComputationsException(this,
					"Wrong key to unlock modifying of '{collectionChangeAction.ToString()}' group change action.");
		}

		public bool IsModifyGroupChangeActionLocked(CollectionChangeAction collectionChangeAction)
		{
			return lockModifyGroupChangeActionsKeys.ContainsKey(collectionChangeAction);
		}

		private void checkLockModifyGroupChangeAction(CollectionChangeAction collectionChangeAction)
		{
			if (lockModifyGroupChangeActionsKeys.ContainsKey(collectionChangeAction))
				throw new ObservableComputationsException(this,
					"Modifying of '{collectionChangeAction.ToString()}' group change action is locked. Unlock first.");
		}



		private PropertyChangedEventHandler _sourceScalarPropertyChangedEventHandler;
		private WeakPropertyChangedEventHandler _sourceScalarWeakPropertyChangedEventHandler;

		private readonly bool _keySelectorExpressionContainsParametrizedObservableComputationsCalls;

		private readonly ExpressionWatcher.ExpressionInfo _keySelectorExpressionInfo;

		private PropertyChangedEventHandler _equalityComparerScalarPropertyChangedEventHandler;
		private WeakPropertyChangedEventHandler _equalityComparerScalarWeakPropertyChangedEventHandler;

		private ObservableCollectionWithChangeMarker<TSourceItem> _sourceAsList;
		bool _rootSourceWrapper;

		private bool _lastProcessedSourceChangeMarker;
		private Queue<ExpressionWatcher.Raise> _deferredExpressionWatcherChangedProcessings;

		private NotifyCollectionChangedEventHandler _sourceNotifyCollectionChangedEventHandler;
		private WeakNotifyCollectionChangedEventHandler _sourceWeakNotifyCollectionChangedEventHandler;

		private Dictionary<TKey, Group<TSourceItem, TKey>> _groupDictionary;
		private Group<TSourceItem, TKey> _nullGroup;

		Positions<ItemInfo> _sourcePositions;
		private List<ItemInfo> _itemInfos;

		Positions<Position> _resultPositions;
		readonly int _initialResultCapacity;

		internal Action<Group<TSourceItem, TKey>, int, TSourceItem> _insertItemIntoGroupAction;
		internal Action<Group<TSourceItem, TKey>, int> _removeItemFromGroupAction;
		internal Action<Group<TSourceItem, TKey>, int, TSourceItem> _setGroupItemAction;
		internal Action<Group<TSourceItem, TKey>, int, int> _moveItemInGroupAction;
		internal Action<Group<TSourceItem, TKey>> _clearGroupItemsAction;

		internal readonly IReadScalar<INotifyCollectionChanged> _sourceScalar;
		internal INotifyCollectionChanged _source;
		internal readonly Expression<Func<TSourceItem, TKey>> _keySelectorExpression;
		internal readonly Expression<Func<TSourceItem, TKey>> _keySelectorExpressionOriginal;

		internal readonly IReadScalar<IEqualityComparer<TKey>> _equalityComparerScalar;
		internal IEqualityComparer<TKey> _equalityComparer;
		private readonly Func<TSourceItem, TKey> _keySelectorFunc;

		private sealed class ItemInfo : Position
		{
			public TKey Key;
			public ExpressionWatcher ExpressionWatcher;
			public Func<TKey> SelectorFunc;
		}

		[ObservableComputationsCall]
		public Grouping(
			IReadScalar<INotifyCollectionChanged> sourceScalar,
			Expression<Func<TSourceItem, TKey>> keySelectorExpression,
			IReadScalar<IEqualityComparer<TKey>> equalityComparerScalar = null,
			int capacity = 0) : this(keySelectorExpression, Utils.getCapacity(sourceScalar), capacity)
		{
			_equalityComparerScalar = equalityComparerScalar;
			initializeEqualityComparer();

			_sourceScalar = sourceScalar;
			initializeSourceScalar();

			_groupDictionary = new Dictionary<TKey, Group<TSourceItem, TKey>>(_equalityComparer);
			initializeFromSource();
		}


		[ObservableComputationsCall]
		public Grouping(
			INotifyCollectionChanged source,
			Expression<Func<TSourceItem, TKey>> keySelectorExpression,
			IReadScalar<IEqualityComparer<TKey>> equalityComparerScalar = null,
			int capacity = 0) : this(keySelectorExpression, Utils.getCapacity(source), capacity)
		{
			_equalityComparerScalar = equalityComparerScalar;
			initializeEqualityComparer();

			_source = source;

			_groupDictionary = new Dictionary<TKey, Group<TSourceItem, TKey>>(_equalityComparer);
			initializeFromSource();
		}

		[ObservableComputationsCall]
		public Grouping(
			IReadScalar<INotifyCollectionChanged> sourceScalar,
			Expression<Func<TSourceItem, TKey>> keySelectorExpression,
			IEqualityComparer<TKey> equalityComparer = null,
			int capacity = 0) : this(keySelectorExpression, Utils.getCapacity(sourceScalar), capacity)
		{
			_equalityComparer = equalityComparer ?? EqualityComparer<TKey>.Default;

			_sourceScalar = sourceScalar;
			initializeSourceScalar();

			_groupDictionary = new Dictionary<TKey, Group<TSourceItem, TKey>>(_equalityComparer);
			initializeFromSource();
		}

		[ObservableComputationsCall]
		public Grouping(
			INotifyCollectionChanged source,
			Expression<Func<TSourceItem, TKey>> keySelectorExpression,
			IEqualityComparer<TKey> equalityComparer = null,
			int capacity = 0) : this(keySelectorExpression, Utils.getCapacity(source), capacity)
		{
			_equalityComparer = equalityComparer ?? EqualityComparer<TKey>.Default;

			_source = source;

			_groupDictionary = new Dictionary<TKey, Group<TSourceItem, TKey>>(_equalityComparer);
			initializeFromSource();
		}

		private Grouping(
			Expression<Func<TSourceItem, TKey>> keySelectorExpression, 
			int sourceCapacity,
			int resultCapacity) : base(resultCapacity)
		{
			_itemInfos = new List<ItemInfo>(sourceCapacity);
			_sourcePositions = new Positions<ItemInfo>(_itemInfos);

			_initialResultCapacity = resultCapacity;
			_resultPositions = new Positions<Position>(new List<Position>(_initialResultCapacity));

			_keySelectorExpressionOriginal = keySelectorExpression;
			CallToConstantConverter callToConstantConverter =
				new CallToConstantConverter(_keySelectorExpressionOriginal.Parameters);
			_keySelectorExpression =
				(Expression<Func<TSourceItem, TKey>>) callToConstantConverter.Visit(_keySelectorExpressionOriginal);
			_keySelectorExpressionContainsParametrizedObservableComputationsCalls =
				callToConstantConverter.ContainsParametrizedObservableComputationCalls;

			if (!_keySelectorExpressionContainsParametrizedObservableComputationsCalls)
			{
				_keySelectorExpressionInfo = ExpressionWatcher.GetExpressionInfo(_keySelectorExpression);
				// ReSharper disable once PossibleNullReferenceException
				_keySelectorFunc = _keySelectorExpression.Compile();
			}
		}

		private void initializeSourceScalar()
		{
			_sourceScalarPropertyChangedEventHandler = handleSourceScalarValueChanged;
			_sourceScalarWeakPropertyChangedEventHandler =
				new WeakPropertyChangedEventHandler(_sourceScalarPropertyChangedEventHandler);
			_sourceScalar.PropertyChanged += _sourceScalarWeakPropertyChangedEventHandler.Handle;
		}

		private void initializeEqualityComparer()
		{
			if (_equalityComparerScalar != null)
			{
				_equalityComparerScalarPropertyChangedEventHandler = handleEqualityComparerScalarValueChanged;
				_equalityComparerScalarWeakPropertyChangedEventHandler =
					new WeakPropertyChangedEventHandler(_equalityComparerScalarPropertyChangedEventHandler);
				_equalityComparerScalar.PropertyChanged += _equalityComparerScalarWeakPropertyChangedEventHandler.Handle;
				_equalityComparer = _equalityComparerScalar.Value ?? EqualityComparer<TKey>.Default;
			}
			else
			{
				_equalityComparer = EqualityComparer<TKey>.Default;
			}
		}

		private void handleEqualityComparerScalarValueChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(IReadScalar<object>.Value)) return;
			checkConsistent(sender, e);

			_handledEventSender = sender;
			_handledEventArgs = e;

			_equalityComparer = _equalityComparerScalar.Value ?? EqualityComparer<TKey>.Default;
			_isConsistent = false;
			initializeFromSource();
			_isConsistent = true;
			raiseConsistencyRestored();

			_handledEventSender = null;
			_handledEventArgs = null;
		}

		private void handleSourceScalarValueChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(IReadScalar<object>.Value)) return;
			checkConsistent(sender, e);

			_handledEventSender = sender;
			_handledEventArgs = e;

			_isConsistent = false;
			initializeFromSource();
			_isConsistent = true;
			raiseConsistencyRestored();

			_handledEventSender = null;
			_handledEventArgs = null;
		}

		private void handleSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			checkConsistent(sender, e);
			if (!_rootSourceWrapper && _lastProcessedSourceChangeMarker == _sourceAsList.ChangeMarkerField) return;

			_handledEventSender = sender;
			_handledEventArgs = e;

			_lastProcessedSourceChangeMarker = !_lastProcessedSourceChangeMarker;

			_isConsistent = false;

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					int newIndex1 = e.NewStartingIndex;
					TSourceItem addedItem = _sourceAsList[newIndex1];
					registerSourceItem(addedItem, false, _sourcePositions.Insert(newIndex1));
					break;
				case NotifyCollectionChangedAction.Remove:
					unregisterSourceItem(e.OldStartingIndex, true);
					break;
				case NotifyCollectionChangedAction.Replace:
					int replacingSourceIndex = e.NewStartingIndex;
					TSourceItem newItem = _sourceAsList[replacingSourceIndex];
					ItemInfo replacingItemInfo = _itemInfos[replacingSourceIndex];

					getNewExpressionWatcherAndKeySelectorFunc(newItem, out ExpressionWatcher watcher, out Func<TKey> selectorFunc);

					TKey key = replacingItemInfo.Key;
					if (_equalityComparer.Equals(key, applyKeySelector(newItem, selectorFunc)))
					{
						ExpressionWatcher oldExpressionWatcher = replacingItemInfo.ExpressionWatcher;
						oldExpressionWatcher.Dispose();

						replacingItemInfo.SelectorFunc = selectorFunc;
						replacingItemInfo.ExpressionWatcher = watcher;
						watcher.ValueChanged = expressionWatcher_OnValueChanged;
						watcher._position = oldExpressionWatcher._position;

						Group<TSourceItem, TKey> @group;

						@group = key != null 
							? _groupDictionary[key] 
							: _nullGroup;

						@group.baseSetItem(findIndexInGroup(replacingSourceIndex, @group), newItem);
					}
					else
					{
						unregisterSourceItem(replacingSourceIndex, false);
						registerSourceItem(newItem, false, replacingItemInfo);	
					}
					break;
				case NotifyCollectionChangedAction.Move:
					int oldStartingIndex = e.OldStartingIndex;
					int newStartingIndex = e.NewStartingIndex;
					if (oldStartingIndex != newStartingIndex)
					{
						ItemInfo itemInfo = _itemInfos[oldStartingIndex];

						Group<TSourceItem, TKey> group1;

						group1 = itemInfo.Key != null 
							? _groupDictionary[itemInfo.Key] 
							: _nullGroup;

						int oldIndex = findIndexInGroup(oldStartingIndex, group1);
						_sourcePositions.Move(oldStartingIndex, newStartingIndex);

						int? newIndex = null;

						if (group1.Count > 1)
						{
							List<Position> group1SourcePositions = group1._sourcePositions;
							int sourcePositionsCount = group1SourcePositions.Count;
							if (oldIndex == 0)
							{
								if (group1SourcePositions[0].Index <= newStartingIndex)
								{
									newIndex = findInsertingIndexInGroup(newStartingIndex, group1, 
										           1, sourcePositionsCount - 1) - 1;
								}
								else
								{
									newIndex = 0;
								}
							}
							else if (oldIndex == sourcePositionsCount - 1)
							{
								if (group1SourcePositions[oldIndex].Index >= newStartingIndex)
								{
									newIndex = findInsertingIndexInGroup(newStartingIndex, group1,
										0, sourcePositionsCount - 2);
								}
								else
								{
									newIndex = sourcePositionsCount - 1;
								}
							}
							else
							{
								int comparisonsSum = 
									(newStartingIndex - group1SourcePositions[oldIndex - 1].Index > 0 ? 1 : -1)
									+ (newStartingIndex - group1SourcePositions[oldIndex + 1].Index >= 0 ? 1 : -1);

								if (comparisonsSum != 0)
								{
									if (comparisonsSum == 2)
									{
										newIndex = findInsertingIndexInGroup(newStartingIndex, group1, 
											           oldIndex + 1, sourcePositionsCount - 1) - 1;	
									}
									else //if (comparisonsSum == -2)
									{
										newIndex = findInsertingIndexInGroup(newStartingIndex, group1, 
											0, oldIndex - 1);								
									}
								}
							}

							if (newIndex.HasValue)
							{
								Position movingPosition = group1SourcePositions[oldIndex];
								group1SourcePositions.RemoveAt(oldIndex);
								group1SourcePositions.Insert(newIndex.Value, movingPosition);
								group1.baseMoveItem(oldIndex, newIndex.Value);
							}			
						}

						moveGroupToActualIndex(group1);
					}
							
					break;
				case NotifyCollectionChangedAction.Reset:
					initializeFromSource();
					break;
			}

			if (_deferredExpressionWatcherChangedProcessings != null)
				while (_deferredExpressionWatcherChangedProcessings.Count > 0)
				{
					ExpressionWatcher.Raise expressionWatcherRaise = _deferredExpressionWatcherChangedProcessings.Dequeue();
					if (!expressionWatcherRaise.ExpressionWatcher._disposed)
					{
						_handledEventSender = expressionWatcherRaise.EventSender;
						_handledEventArgs = expressionWatcherRaise.EventArgs;
						processExpressionWatcherValueChanged(expressionWatcherRaise.ExpressionWatcher);
					}
				} 

			_isConsistent = true;
			raiseConsistencyRestored();

			_handledEventSender = null;
			_handledEventArgs = null;
		}

		private void expressionWatcher_OnValueChanged(ExpressionWatcher expressionWatcher, object sender, EventArgs eventArgs)
		{
			checkConsistent(sender, eventArgs);

			_handledEventSender = sender;
			_handledEventArgs = eventArgs;

			if (_rootSourceWrapper || _sourceAsList.ChangeMarkerField ==_lastProcessedSourceChangeMarker)
			{
				_isConsistent = false;
				processExpressionWatcherValueChanged(expressionWatcher);
				_isConsistent = true;
				raiseConsistencyRestored();
			}
			else
			{
				(_deferredExpressionWatcherChangedProcessings = _deferredExpressionWatcherChangedProcessings 
					??  new Queue<ExpressionWatcher.Raise>())
				.Enqueue(new ExpressionWatcher.Raise(expressionWatcher, sender, eventArgs));
			}

			_handledEventSender = null;
			_handledEventArgs = null;
		}

		private void initializeFromSource()
		{			
			int originalCount = _items.Count;

			if (_sourceNotifyCollectionChangedEventHandler != null)
			{
				int itemInfosCount = _itemInfos.Count;
				for (int index = 0; index < itemInfosCount; index++)
				{
					ItemInfo itemInfo = _itemInfos[index];
					ExpressionWatcher expressionWatcher = itemInfo.ExpressionWatcher;
					expressionWatcher.Dispose();
				}

				int sourceCapacity = _sourceScalar != null ? Utils.getCapacity(_sourceScalar) : Utils.getCapacity(_source);
				_itemInfos = new List<ItemInfo>(sourceCapacity);
				_sourcePositions = new Positions<ItemInfo>(_itemInfos);

				_resultPositions = new Positions<Position>(new List<Position>(_initialResultCapacity));
				_nullGroup = null;

				_groupDictionary = new Dictionary<TKey, Group<TSourceItem, TKey>>(_equalityComparer);			

				if (_rootSourceWrapper)
				{
					_sourceAsList.CollectionChanged -= _sourceNotifyCollectionChangedEventHandler;
				}
				else
				{
					_sourceAsList.CollectionChanged -= _sourceWeakNotifyCollectionChangedEventHandler.Handle;
					_sourceWeakNotifyCollectionChangedEventHandler = null;					
				}

				_sourceNotifyCollectionChangedEventHandler = null;

				_items.Clear();
			}

			if (_sourceScalar != null) _source = _sourceScalar.Value;
			_sourceAsList = null;

			if (_source != null)
			{
				if (_source is ObservableCollectionWithChangeMarker<TSourceItem> sourceAsList)
				{
					_sourceAsList = sourceAsList;
					_rootSourceWrapper = false;
				}
				else
				{
					_sourceAsList = new RootSourceWrapper<TSourceItem>(_source);
					_rootSourceWrapper = true;
				}

				_lastProcessedSourceChangeMarker = _sourceAsList.ChangeMarkerField;

				int count = _sourceAsList.Count;
				for (int index = 0; index < count; index++)
				{
					TSourceItem sourceItem = _sourceAsList[index];
					registerSourceItem(sourceItem, true, _sourcePositions.Insert(index), true);
				}

				_sourceNotifyCollectionChangedEventHandler = handleSourceCollectionChanged;

				if (_rootSourceWrapper)
				{
					_sourceAsList.CollectionChanged += _sourceNotifyCollectionChangedEventHandler;
				}
				else
				{
					_sourceWeakNotifyCollectionChangedEventHandler = 
						new WeakNotifyCollectionChangedEventHandler(_sourceNotifyCollectionChangedEventHandler);

					_sourceAsList.CollectionChanged += _sourceWeakNotifyCollectionChangedEventHandler.Handle;
				}
			}			

			reset();
		}


		private void processExpressionWatcherValueChanged(ExpressionWatcher expressionWatcher)
		{
			int sourceIndex = expressionWatcher._position.Index;
			TSourceItem sourceItem = _sourceAsList[sourceIndex];
			ItemInfo itemInfo = _itemInfos[sourceIndex];
			TKey oldKey = itemInfo.Key;
			TKey newKey = applyKeySelector(sourceIndex);

			if (!_equalityComparer.Equals(oldKey, newKey))
			{
				itemInfo.Key = newKey;

				removeSourceItemFromGroup(sourceIndex, oldKey);
				addSourceItemToGroup(sourceItem, expressionWatcher._position, false, newKey);
			}
		}

		private void registerSourceItem(TSourceItem sourceItem, bool addNewToEnd, ItemInfo itemInfo, bool initializing = false)
		{
			getNewExpressionWatcherAndKeySelectorFunc(sourceItem, out ExpressionWatcher watcher, out Func<TKey> selectorFunc);

			itemInfo.SelectorFunc = selectorFunc;
			itemInfo.ExpressionWatcher = watcher;
			watcher.ValueChanged = expressionWatcher_OnValueChanged;
			watcher._position = itemInfo;

			TKey key = applyKeySelector(itemInfo.Index);
			itemInfo.Key = key;

			addSourceItemToGroup(sourceItem, itemInfo, addNewToEnd, key, initializing);
		}

		private void unregisterSourceItem(int sourceIndex, bool removeFromSourcePositions)
		{
			ItemInfo itemInfo = _itemInfos[sourceIndex];
			TKey key = itemInfo.Key;

			removeSourceItemFromGroup(sourceIndex, key);	
			
			ExpressionWatcher watcher = itemInfo.ExpressionWatcher;
			watcher.Dispose();

			if (removeFromSourcePositions)
			{
				_sourcePositions.Remove(sourceIndex);				
			}
		}

		private void addSourceItemToGroup(
			TSourceItem sourceItem,
			Position sourceItemPosition, 
			bool addNewToEnd,
			TKey key,
			bool initializing = false)
		{
			if ((key != null && !_groupDictionary.ContainsKey(key))
			    || (key == null && _nullGroup == null))
			{
				Position resultItemPosition;

				resultItemPosition = !addNewToEnd 
					? _resultPositions.Insert(findInsertingResultIndex(sourceItemPosition.Index)) 
					: _resultPositions.Add();

				Group<TSourceItem, TKey> newGroup = getNewGroup(sourceItem, sourceItemPosition, key, resultItemPosition);

				if (key != null)
					_groupDictionary.Add(key, newGroup);
				else
					_nullGroup = newGroup;

				if (initializing)
					_items.Insert(resultItemPosition.Index, newGroup);
				else
					baseInsertItem(resultItemPosition.Index, newGroup);
			}
			else
			{
				Group<TSourceItem, TKey> existingGroup;

				existingGroup = key != null
					? _groupDictionary[key]
					: _nullGroup;

				int newIndex = findInsertingIndexInGroup(sourceItemPosition.Index, existingGroup);

				existingGroup._sourcePositions.Insert(newIndex, sourceItemPosition);
				existingGroup.baseInsertItem(newIndex, sourceItem);

				moveGroupToActualIndex(existingGroup, initializing);
			}
		}

		internal virtual Group<TSourceItem, TKey> getNewGroup(TSourceItem sourceItem, Position sourceItemPosition, TKey key, Position resultItemPosition)
		{
			return new Group<TSourceItem, TKey>(this, key, resultItemPosition, sourceItemPosition, sourceItem);
		}

		private void moveGroupToActualIndex(
			Group<TSourceItem, TKey> existingGroup,
			bool initializing = false)
		{
			int? newResultIndex = null;

			int count = Count;

			List<Position> existingGroupSourcePositions = existingGroup._sourcePositions;
			int positionIndex = existingGroup._position.Index;
			if (!(
				count == 1
				|| (
					positionIndex == 0
					&& existingGroupSourcePositions[0].Index < this[1]._sourcePositions[0].Index)
				|| (
					positionIndex == count - 1
					&& existingGroupSourcePositions[0].Index > this[count - 1]._sourcePositions[0].Index)))
			{
				if (positionIndex == 0)
				{
					newResultIndex = findInsertingResultIndex(existingGroupSourcePositions[0].Index, 1, count - 1) - 1;
				}
				else if (positionIndex == count - 1)
				{
					newResultIndex = findInsertingResultIndex(existingGroupSourcePositions[0].Index, 0, count - 2);
				}
				else
				{
					int comparisonsSum =
						(existingGroupSourcePositions[0].Index - this[positionIndex - 1]._sourcePositions[0].Index > 0
							? 1
							: -1)
						+ (existingGroupSourcePositions[0].Index - this[positionIndex + 1]._sourcePositions[0].Index >= 0
							? 1
							: -1);

					if (comparisonsSum != 0)
					{
						if (comparisonsSum == 2)
						{
							newResultIndex = findInsertingResultIndex(
								existingGroupSourcePositions[0].Index,
								positionIndex + 1, count - 1) - 1;
						}
						else //if (comparisonsSum == -2)
						{
							newResultIndex = findInsertingResultIndex(
								existingGroupSourcePositions[0].Index,
								0, positionIndex - 1);
						}
					}
				}
			}

			if (newResultIndex.HasValue)
			{
				int oldIndex = positionIndex;
				_resultPositions.Move(positionIndex, newResultIndex.Value);

				if (initializing)
				{
					_items.RemoveAt(oldIndex);
					_items.Insert(newResultIndex.Value, existingGroup);
				}
				else
					baseMoveItem(oldIndex, newResultIndex.Value);
			}
		}

		// binary search
		private static int findInsertingIndexInGroup(
			int sourceIndex,
			Group<TSourceItem, TKey> @group,
			int lowerIndex,
			int upperIndex)
		{
			int? newIndex;

			do
			{
				int middleIndex;
				int length = upperIndex - lowerIndex + 1;
				if (length == 0)
				{
					newIndex = 0;
				}
				else
				{
					List<Position> groupSourcePositions = @group._sourcePositions;
					if (length == 1)
					{
						newIndex = groupSourcePositions[lowerIndex].Index > sourceIndex ? lowerIndex : lowerIndex + 1;
					}
					else if (length == 2)
					{
						newIndex = groupSourcePositions[lowerIndex].Index > sourceIndex 
							? lowerIndex
							: groupSourcePositions[upperIndex].Index > sourceIndex
								? upperIndex
								: upperIndex + 1;
					}
					else
					{
						middleIndex = lowerIndex + (length >> 1);

						int middleSourceIndex = groupSourcePositions[middleIndex].Index;
						int increment = (middleSourceIndex < sourceIndex ? 1 : -1);
						newIndex =
							(sourceIndex - middleSourceIndex > 0 ? 1 : -1)
							+ (sourceIndex - groupSourcePositions[middleIndex + increment].Index > 0 ? 1 : -1) == 0 		 
								? increment == 1 
									? middleIndex + 1
									: middleIndex
								: (int?) null;

						if (!newIndex.HasValue)
						{
							if (increment > 0)
							{
								lowerIndex = middleIndex;
							}
							else
							{
								upperIndex = middleIndex;
							}
						}
					}
				}
			} while (!newIndex.HasValue);

			return newIndex.Value;
		}

		private static int findInsertingIndexInGroup(
			int sourceIndex,
			Group<TSourceItem, TKey> @group)
		{
			return findInsertingIndexInGroup(
				sourceIndex, 
				@group, 
				0, @group._sourcePositions.Count - 1);
		}

		private static int findIndexInGroup(
			int sourceIndex,
			Group<TSourceItem, TKey> @group)
		{
			int lowerIndex = 0;
			List<Position> groupSourcePositions = @group._sourcePositions;
			int upperIndex = groupSourcePositions.Count - 1;

			do
			{
				int middleIndex;
				int length = upperIndex - lowerIndex + 1;
				/*if (length == 0)
				{
					throw new ObservableComputationsException(this, "Inner exception");
				}
				else */
				if (length == 1)
				{
					return 0;
				}
				else if (length == 2)
				{
					if (groupSourcePositions[lowerIndex].Index == sourceIndex) return lowerIndex;
					else if (groupSourcePositions[upperIndex].Index == sourceIndex) return upperIndex;
					//else throw new ObservableComputationsException(this, "Inner exception");
				}
				else
				{
					middleIndex = lowerIndex + (length >> 1);

					int middleSourceIndex = groupSourcePositions[middleIndex].Index;

					if (middleSourceIndex == sourceIndex)
						return middleIndex;
					else if (middleSourceIndex < sourceIndex)
					{
						lowerIndex = middleIndex;
					}
					else
					{
						upperIndex = middleIndex;
					}
				}
			} while (true);
		}

		// binary search
		private int findInsertingResultIndex(int sourceIndex, int lowerIndex, int upperIndex)
		{
			int? resultIndex;
			do
			{
				int middleIndex;
				int length = upperIndex - lowerIndex + 1;
				if (length == 0)
				{
					resultIndex = 0;
				}
				else
				{
					int index = this[lowerIndex]._sourcePositions[0].Index;

					if (length == 1)
					{
						resultIndex = index > sourceIndex
							? lowerIndex
							: lowerIndex + 1;
					}
					else if (length == 2)
					{
						resultIndex = index > sourceIndex
							? lowerIndex
							: this[upperIndex]._sourcePositions[0].Index > sourceIndex
								? upperIndex
								: upperIndex + 1;
					}
					else
					{
						middleIndex = lowerIndex + (length >> 1);

						int middleSourceIndex = this[middleIndex]._sourcePositions[0].Index;
						int increment = (middleSourceIndex < sourceIndex ? 1 : -1);
						resultIndex =
							(sourceIndex - middleSourceIndex > 0 ? 1 : -1)
							+ (sourceIndex
							   - this[middleIndex + increment]._sourcePositions[0].Index > 0
								? 1 : -1)
							== 0
								? increment == 1
									? middleIndex + 1
									: middleIndex
								: (int?)null;

						if (resultIndex == null)
						{
							if (increment > 0)
							{
								lowerIndex = middleIndex;
							}
							else
							{
								upperIndex = middleIndex;
							}
						}
					}
				}
			} while (!resultIndex.HasValue);

			return resultIndex.Value;
		}

		private int findInsertingResultIndex(int sourceIndex)
		{
			return findInsertingResultIndex(sourceIndex, 0, Count - 1);
		}

		private void getNewExpressionWatcherAndKeySelectorFunc(TSourceItem sourceItem, out ExpressionWatcher watcher,
			out Func<TKey> keySelectorFunc)
		{
			keySelectorFunc = null;

			if (!_keySelectorExpressionContainsParametrizedObservableComputationsCalls)
			{
				watcher = new ExpressionWatcher(_keySelectorExpressionInfo, sourceItem);
			}
			else
			{
				Expression<Func<TKey>> deparametrizedSelectorExpression =
					(Expression<Func<TKey>>) _keySelectorExpression.ApplyParameters(new object[] {sourceItem});
				Expression<Func<TKey>> selectorExpression =
					(Expression<Func<TKey>>)
						new CallToConstantConverter().Visit(deparametrizedSelectorExpression);
				// ReSharper disable once PossibleNullReferenceException
				keySelectorFunc = selectorExpression.Compile();
				watcher = new ExpressionWatcher(ExpressionWatcher.GetExpressionInfo(selectorExpression));
			}
		}

		private void removeSourceItemFromGroup(int sourceIndex, TKey key)
		{
			Group<TSourceItem, TKey> removingGroup;
			removingGroup = key != null ? _groupDictionary[key] : _nullGroup;

			int indexInDistinctingValueInfoSourcePositions = findIndexInGroup(sourceIndex, removingGroup);
			removingGroup._sourcePositions.RemoveAt(
				indexInDistinctingValueInfoSourcePositions);
			removingGroup.baseRemoveItem(indexInDistinctingValueInfoSourcePositions);

			if (removingGroup.Count == 0)
			{
				if (key != null) _groupDictionary.Remove(key);
				else _nullGroup = null;

				int positionIndex = removingGroup._position.Index;
				_resultPositions.Remove(positionIndex);
				baseRemoveItem(positionIndex);
			}
			else
			{
				moveGroupToActualIndex(removingGroup);				
			}
		}

		private TKey applyKeySelector(int index)
		{
			if (Configuration.TrackComputingsExecutingUserCode)
			{
				Thread currentThread = Thread.CurrentThread;
				DebugInfo._computingsExecutingUserCode.TryGetValue(currentThread, out IComputing computing);
				DebugInfo._computingsExecutingUserCode[currentThread] = this;	
				_userCodeIsCalledFrom = computing;

				TKey result = _keySelectorExpressionContainsParametrizedObservableComputationsCalls 
					? _itemInfos[index].SelectorFunc() 
					: _keySelectorFunc(_sourceAsList[index]);;

				if (computing == null) DebugInfo._computingsExecutingUserCode.TryRemove(currentThread, out IComputing _);
				else DebugInfo._computingsExecutingUserCode[currentThread] = computing;
				_userCodeIsCalledFrom = null;
				return result;
			}

			return _keySelectorExpressionContainsParametrizedObservableComputationsCalls 
				? _itemInfos[index].SelectorFunc() 
				: _keySelectorFunc(_sourceAsList[index]);
		}

		public TKey ApplyKeySelector(int index)
		{
			if (Configuration.TrackComputingsExecutingUserCode)
			{
				Thread currentThread = Thread.CurrentThread;
				DebugInfo._computingsExecutingUserCode.TryGetValue(currentThread, out IComputing computing);
				DebugInfo._computingsExecutingUserCode[currentThread] = this;	
				_userCodeIsCalledFrom = computing;

				TKey result = applyKeySelector(index);

				if (computing == null) DebugInfo._computingsExecutingUserCode.TryRemove(currentThread, out IComputing _);
				else DebugInfo._computingsExecutingUserCode[currentThread] = computing;
				_userCodeIsCalledFrom = null;
				return result;
			}

			return applyKeySelector(index);
		}

		private TKey applyKeySelector(TSourceItem sourceItem, Func<TKey> selectorFunc)
		{
			if (Configuration.TrackComputingsExecutingUserCode)
			{
				Thread currentThread = Thread.CurrentThread;
				DebugInfo._computingsExecutingUserCode.TryGetValue(currentThread, out IComputing computing);
				DebugInfo._computingsExecutingUserCode[currentThread] = this;	
				_userCodeIsCalledFrom = computing;

				TKey result = _keySelectorExpressionContainsParametrizedObservableComputationsCalls 
					? selectorFunc() 
					: _keySelectorFunc(sourceItem);

				if (computing == null) DebugInfo._computingsExecutingUserCode.TryRemove(currentThread, out IComputing _);
				else DebugInfo._computingsExecutingUserCode[currentThread] = computing;
				_userCodeIsCalledFrom = null;
				return result;
			}

			return _keySelectorExpressionContainsParametrizedObservableComputationsCalls 
				? selectorFunc() 
				: _keySelectorFunc(sourceItem);
		}

		public Group<TSourceItem, TKey> GetGroup(TKey key)
		{
			return getGroup(key);
		}

		internal Group<TSourceItem, TKey> getGroup(TKey key)
		{
			return key == null
				? _nullGroup
				: _groupDictionary.TryGetValue(key, out Group<TSourceItem, TKey> group)
					? @group
					: null;
		}

		~Grouping()
		{
			if (_sourceWeakNotifyCollectionChangedEventHandler != null)
			{
				_sourceAsList.CollectionChanged -= _sourceWeakNotifyCollectionChangedEventHandler.Handle;			
			}

			if (_equalityComparerScalarWeakPropertyChangedEventHandler != null)
			{
				_equalityComparerScalar.PropertyChanged -= _equalityComparerScalarWeakPropertyChangedEventHandler.Handle;			
			}

			if (_sourceScalarWeakPropertyChangedEventHandler != null)
			{
				_sourceScalar.PropertyChanged -= _sourceScalarWeakPropertyChangedEventHandler.Handle;			
			}
		}

		public void ValidateConsistency()
		{
			_resultPositions.ValidateConsistency();
			_sourcePositions.ValidateConsistency();

			IList<TSourceItem> source = _sourceScalar.getValue(_source, new ObservableCollection<TSourceItem>()) as IList<TSourceItem>;
			IEqualityComparer<TKey> equalityComparer = _equalityComparerScalar.getValue(_equalityComparer) ?? EqualityComparer<TKey>.Default;
			List<Tuple<TKey, List<Tuple<TSourceItem, int>>>> result = new List<Tuple<TKey, List<Tuple<TSourceItem, int>>>>();
			Func<TSourceItem, TKey> selector = _keySelectorExpressionOriginal.Compile();

			// ReSharper disable once PossibleNullReferenceException
			if (_itemInfos.Count != source.Count)
				throw new ObservableComputationsException(this, "Consistency violation: Grouping.14");

			if (_resultPositions.List.Count != Count)
				throw new ObservableComputationsException(this, "Consistency violation: Grouping.15");

			for (int sourceIndex = 0; sourceIndex < source.Count; sourceIndex++)
			{
				TSourceItem sourceItem = source[sourceIndex];
				TKey key = selector(sourceItem);

				Tuple<TKey, List<Tuple<TSourceItem, int>>> resultItem = result.SingleOrDefault(ri => equalityComparer.Equals(ri.Item1, key));
				if (resultItem == null)
				{
					result.Add(new Tuple<TKey, List<Tuple<TSourceItem, int>>>(key, new List<Tuple<TSourceItem, int>>(new []{new Tuple<TSourceItem, int>(sourceItem, sourceIndex)})));
				}
				else
				{
					resultItem.Item2.Add(new Tuple<TSourceItem, int>(sourceItem, sourceIndex));
				}
				
				if (!equalityComparer.Equals(_itemInfos[sourceIndex].Key, key))
					throw new ObservableComputationsException(this, "Consistency violation: Grouping.1");

				if (_itemInfos[sourceIndex].ExpressionWatcher._position.Index != sourceIndex)
					throw new ObservableComputationsException(this, "Consistency violation: Grouping.2");
			}

			if (result.Count != Count) throw new ObservableComputationsException(this, "Consistency violation: Grouping.3");

			for (int thisIndex = 0; thisIndex < Count; thisIndex++)
			{
				Group<TSourceItem, TKey> @group = this[thisIndex];
				Tuple<TKey, List<Tuple<TSourceItem, int>>> resultItem = result[thisIndex];

				if (!equalityComparer.Equals(@group.Key, resultItem.Item1)) throw new ObservableComputationsException(this, "Consistency violation: Grouping.4");
				if (@group.Count != resultItem.Item2.Count) throw new ObservableComputationsException(this, "Consistency violation: Grouping.5");

				for (int groupIndex = 0; groupIndex < @group.Count; groupIndex++)
				{
					TSourceItem sourceItem = @group[groupIndex];
					Tuple<TSourceItem, int> resultItemItem = resultItem.Item2[groupIndex];

					if (!EqualityComparer<TSourceItem>.Default.Equals(sourceItem, resultItemItem.Item1)) throw new ObservableComputationsException(this, "Consistency violation: Grouping.6");
					if (@group._sourcePositions[groupIndex].Index != resultItemItem.Item2) throw new ObservableComputationsException(this, "Consistency violation: Grouping.7");
				}

				if (@group._position.Index != thisIndex) throw new ObservableComputationsException(this, "Consistency violation: Grouping.8");

				if (resultItem.Item1 != null)
				{
					Group<TSourceItem, TKey> groupFromDictionary = _groupDictionary[resultItem.Item1];
					if (groupFromDictionary != @group) throw new ObservableComputationsException(this, "Consistency violation: Grouping.9");					
				}
				else
				{
					if (_nullGroup != @group) throw new ObservableComputationsException(this, "Consistency violation: Grouping.10");	
				}

				if (!_resultPositions.List.Contains(@group._position))
					throw new ObservableComputationsException(this, "Consistency violation: Grouping.12");

			}		
			
			if (_nullGroup != null && !_resultPositions.List.Contains(_nullGroup._position))
				throw new ObservableComputationsException(this, "Consistency violation: Grouping.13");
		}
	}

	public class Group<TSourceItem, TKey> : CollectionComputingChild<TSourceItem>
	{
		public TKey Key => _key;

		// ReSharper disable once MemberCanBePrivate.Global
		public Grouping<TSourceItem, TKey> Grouping => _grouping;
		internal readonly List<Position> _sourcePositions = new List<Position>();
		internal readonly Position _position;
		internal List<CollectionComputingChild<TSourceItem>> _copies;
		internal readonly TKey _key;
		private readonly Grouping<TSourceItem, TKey> _grouping;

		internal Group(Grouping<TSourceItem, TKey> grouping, TKey key, Position resultItemPosition, Position firstSourceIndex, TSourceItem firstSourceItem)
		{
			_grouping = grouping;
			_key = key;
			_position = resultItemPosition;
			_sourcePositions.Add(firstSourceIndex);
			baseInsertItem(0, firstSourceItem);
		}

		internal void baseInsertItem(int index, TSourceItem item)
		{
			if (_copies == null)
			{
				insertItem(index, item);
			}
			else
			{
				insertItemNotExtended(index, item);
				int copiesCount = _copies.Count;
				for (int index1 = 0; index1 < copiesCount; index1++)
				{
					CollectionComputingChild<TSourceItem> copy = _copies[index1];
					copy.insertItem(index, item);
				}
			}
		}

		internal void baseMoveItem(int oldIndex, int newIndex)
		{
			if (_copies == null)
			{
				moveItem(oldIndex, newIndex);
			}
			else
			{
				moveItemNotExtended(oldIndex, newIndex);
				int copiesCount = _copies.Count;
				for (int index = 0; index < copiesCount; index++)
				{
					CollectionComputingChild<TSourceItem> copy = _copies[index];
					copy.moveItem(oldIndex, newIndex);
				}
			}
		}

		internal void baseRemoveItem(int index)
		{
			if (_copies == null)
			{
				removeItem(index);
			}
			else
			{
				removeItemNotExtended(index);
				int copiesCount = _copies.Count;
				for (int index1 = 0; index1 < copiesCount; index1++)
				{
					CollectionComputingChild<TSourceItem> copy = _copies[index1];
					copy.removeItem(index);
				}
			}
		}

		internal void baseSetItem(int index, TSourceItem item)
		{
			if (_copies == null)
			{
				setItem(index, item);
			}
			else
			{
				setItemNotExtended(index, item);
				int copiesCount = _copies.Count;
				for (int index1 = 0; index1 < copiesCount; index1++)
				{
					CollectionComputingChild<TSourceItem> copy = _copies[index1];
					copy.setItem(index, item);
				}
			}
		}

		public override ICollectionComputing Parent => _grouping;

		#region Overrides of ObservableCollection<TResult>

		protected override void InsertItem(int index, TSourceItem item)
		{
			_grouping._insertItemIntoGroupAction(this, index, item);
		}

		protected override void MoveItem(int oldIndex, int newIndex)
		{
			_grouping._moveItemInGroupAction(this, oldIndex, newIndex);
		}

		protected override void RemoveItem(int index)
		{
			_grouping._removeItemFromGroupAction(this, index);
		}

		protected override void SetItem(int index, TSourceItem item)
		{
			_grouping._setGroupItemAction(this, index, item);
		}

		protected override void ClearItems()
		{
			_grouping._clearGroupItemsAction(this);
		}
		#endregion
	}
}