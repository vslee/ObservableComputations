﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using IBCode.ObservableCalculations.Common;
using IBCode.ObservableCalculations.Common.Interface;

namespace IBCode.ObservableCalculations
{
	public class ItemCalculating<TSourceItem> : ScalarCalculating<TSourceItem>, IHasSources
	{
		public IReadScalar<INotifyCollectionChanged> SourceScalar => _sourceScalar;

		// ReSharper disable once MemberCanBeProtected.Global
		public IReadScalar<TSourceItem> DefaultValueScalar => _defaultValueScalar;

		// ReSharper disable once MemberCanBePrivate.Global
		public IReadScalar<int> IndexValueScalar => _indexValueScalar;

		public INotifyCollectionChanged Source => _source;

		// ReSharper disable once MemberCanBePrivate.Global
		public int Index => _index;

		// ReSharper disable once MemberCanBePrivate.Global
		public bool IsDefaulted => _isDefaulted;

		// ReSharper disable once MemberCanBeProtected.Global
		public TSourceItem DefaultValue => _defaultValue;

		public ReadOnlyCollection<INotifyCollectionChanged> SourcesCollection => new ReadOnlyCollection<INotifyCollectionChanged>(new []{Source});
		public ReadOnlyCollection<IReadScalar<INotifyCollectionChanged>> SourceScalarsCollection => new ReadOnlyCollection<IReadScalar<INotifyCollectionChanged>>(new []{SourceScalar});

		protected readonly IReadScalar<INotifyCollectionChanged> _sourceScalar;
		private PropertyChangedEventHandler _sourceScalarPropertyChangedEventHandler;
		private WeakPropertyChangedEventHandler _sourceScalarWeakPropertyChangedEventHandler;

		private PropertyChangedEventHandler _indexScalarPropertyChangedEventHandler;
		private WeakPropertyChangedEventHandler _indexScalarWeakPropertyChangedEventHandler;

		private PropertyChangedEventHandler _defaultValueScalarPropertyChangedEventHandler;
		private WeakPropertyChangedEventHandler _defaultValueScalarWeakPropertyChangedEventHandler;

		protected INotifyCollectionChanged _source;
		private IList<TSourceItem> _sourceAsList;

		private NotifyCollectionChangedEventHandler _sourceNotifyCollectionChangedEventHandler;
		private WeakNotifyCollectionChangedEventHandler _sourceWeakNotifyCollectionChangedEventHandler;
		internal readonly IReadScalar<TSourceItem> _defaultValueScalar;
		private readonly IReadScalar<int> _indexValueScalar;
		private int _index;
		private bool _isDefaulted;
		internal TSourceItem _defaultValue;

		private void initializeIndexScalar()
		{
			_indexScalarPropertyChangedEventHandler = handleIndexScalarValueChanged;
			_indexScalarWeakPropertyChangedEventHandler =
				new WeakPropertyChangedEventHandler(_indexScalarPropertyChangedEventHandler);
			_indexValueScalar.PropertyChanged += _indexScalarWeakPropertyChangedEventHandler.Handle;
			_index = _indexValueScalar.Value;
		}

		private void initializeDefaultValueScalar()
		{
			if (_defaultValueScalar != null)
			{
				_defaultValueScalarPropertyChangedEventHandler = handleDefaultValueScalarValueChanged;
				_defaultValueScalarWeakPropertyChangedEventHandler =
					new WeakPropertyChangedEventHandler(_defaultValueScalarPropertyChangedEventHandler);
				_defaultValueScalar.PropertyChanged += _defaultValueScalarWeakPropertyChangedEventHandler.Handle;
				_defaultValue = _defaultValueScalar.Value;
			}
		}

		private void initializeSourceScalar()
		{
			_sourceScalarPropertyChangedEventHandler = handleSourceScalarValueChanged;
			_sourceScalarWeakPropertyChangedEventHandler =
				new WeakPropertyChangedEventHandler(_sourceScalarPropertyChangedEventHandler);
			_sourceScalar.PropertyChanged += _sourceScalarWeakPropertyChangedEventHandler.Handle;
		}

		[ObservableCalculationsCall]
		public ItemCalculating(
			IReadScalar<INotifyCollectionChanged> sourceScalar,
			IReadScalar<int> indexScalar, 
			IReadScalar<TSourceItem> defaultValueScalar = null)
		{
			_sourceScalar = sourceScalar;
			initializeSourceScalar();

			_defaultValueScalar = defaultValueScalar;
			initializeDefaultValueScalar();

			_indexValueScalar = indexScalar;
			initializeIndexScalar();

			initializeFromSource();
		}

		[ObservableCalculationsCall]
		public ItemCalculating(
			IReadScalar<INotifyCollectionChanged> sourceScalar,
			int index, 
			IReadScalar<TSourceItem> defaultValueScalar = null)
		{
			_sourceScalar = sourceScalar;
			initializeSourceScalar();

			_index = index;

			_defaultValueScalar = defaultValueScalar;
			initializeDefaultValueScalar();

			initializeFromSource();
		}

		[ObservableCalculationsCall]
		public ItemCalculating(
			IReadScalar<INotifyCollectionChanged> sourceScalar,
			int index, 
			TSourceItem defaultValue = default(TSourceItem))
		{
			_sourceScalar = sourceScalar;
			initializeSourceScalar();

			_index = index;
			_defaultValue = defaultValue;

			initializeFromSource();
		}

		[ObservableCalculationsCall]
		public ItemCalculating(
			IReadScalar<INotifyCollectionChanged> sourceScalar,
			IReadScalar<int> indexScalar, 
			TSourceItem defaultValue = default(TSourceItem))
		{
			_sourceScalar = sourceScalar;
			initializeSourceScalar();

			_defaultValue = defaultValue;

			_indexValueScalar = indexScalar;
			initializeIndexScalar();

			initializeFromSource();
		}


		[ObservableCalculationsCall]
		public ItemCalculating(
			INotifyCollectionChanged source,
			IReadScalar<int> indexScalar, 
			IReadScalar<TSourceItem> defaultValueScalar = null)
		{
			_source = source;

			_defaultValueScalar = defaultValueScalar;
			initializeDefaultValueScalar();

			_indexValueScalar = indexScalar;
			initializeIndexScalar();

			initializeFromSource();
		}

		[ObservableCalculationsCall]
		public ItemCalculating(
			INotifyCollectionChanged source,
			int index, 
			IReadScalar<TSourceItem> defaultValueScalar = null)
		{
			_source = source;

			_index = index;

			_defaultValueScalar = defaultValueScalar;
			initializeDefaultValueScalar();

			initializeFromSource();
		}

		[ObservableCalculationsCall]
		public ItemCalculating(
			INotifyCollectionChanged source,
			int index, 
			TSourceItem defaultValue = default(TSourceItem))
		{
			_source = source;
			_index = index;
			_defaultValue = defaultValue;

			initializeFromSource();
		}

		[ObservableCalculationsCall]
		public ItemCalculating(
			INotifyCollectionChanged source,
			IReadScalar<int> indexScalar, 
			TSourceItem defaultValue = default(TSourceItem))
		{
			_source = source;

			_defaultValue = defaultValue;

			_indexValueScalar = indexScalar;
			initializeIndexScalar();

			initializeFromSource();
		}


		private void handleIndexScalarValueChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(IReadScalar<INotifyCollectionChanged>.Value)) return;
			_index = _indexValueScalar.Value;
			recalculateValue();
		}

		private void handleDefaultValueScalarValueChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(IReadScalar<INotifyCollectionChanged>.Value)) return;
			_defaultValue = _defaultValueScalar.Value;
			if (_isDefaulted) setValue(_defaultValue);
		}

		private void handleSourceScalarValueChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(IReadScalar<INotifyCollectionChanged>.Value)) return;
			initializeFromSource();
		}

		private void initializeFromSource()
		{
			if (_sourceNotifyCollectionChangedEventHandler != null)
			{
				_source.CollectionChanged -= _sourceWeakNotifyCollectionChangedEventHandler.Handle;
				_sourceNotifyCollectionChangedEventHandler = null;
				_sourceWeakNotifyCollectionChangedEventHandler = null;
			}

			if (_sourceScalar != null) _source = _sourceScalar.Value;
			_sourceAsList = (IList<TSourceItem>) _source;

			if (_source != null)
			{
				_sourceNotifyCollectionChangedEventHandler = handleSourceCollectionChanged;
				_sourceWeakNotifyCollectionChangedEventHandler =
					new WeakNotifyCollectionChangedEventHandler(_sourceNotifyCollectionChangedEventHandler);

				_source.CollectionChanged += _sourceWeakNotifyCollectionChangedEventHandler.Handle;
			}

			recalculateValue();
		}

		private void recalculateValue()
		{
			if (_sourceAsList != null && _sourceAsList.Count > _index)
			{
				if (_isDefaulted)
				{
					_isDefaulted = false;
					raisePropertyChanged(nameof(IsDefaulted));
				}

				setValue(_sourceAsList[_index]);
			}
			else
			{
				if (!_isDefaulted)
				{
					_isDefaulted = true;
					raisePropertyChanged(nameof(IsDefaulted));
				}

				setValue(_defaultValue);
			}
		}


		private void handleSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewItems.Count > 1) throw new Exception("Adding of multiple items is not supported");
					if (e.NewStartingIndex <= _index) recalculateValue();	
					break;
				case NotifyCollectionChangedAction.Remove:
					if (e.OldItems.Count > 1) throw new Exception("Removing of multiple items is not supported");
					if (e.OldStartingIndex <= _index) recalculateValue();	
					break;
				case NotifyCollectionChangedAction.Replace:
					if (e.NewItems.Count > 1) throw new Exception("Replacing of multiple items is not supported");
					if (e.OldStartingIndex == _index) recalculateValue();
					break;
				case NotifyCollectionChangedAction.Move:
					int oldStartingIndex = e.OldStartingIndex;
					int newStartingIndex = e.NewStartingIndex;
					if (newStartingIndex == oldStartingIndex) return;
					if (newStartingIndex < oldStartingIndex)
					{
						if (_index >= newStartingIndex && _index <= oldStartingIndex)
							setValue(_sourceAsList[_index]);
					}
					else
					{
						if (_index >= oldStartingIndex && _index <= newStartingIndex)
							setValue(_sourceAsList[_index]);						
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					initializeFromSource();
					break;
			}						
		}

		~ItemCalculating()
		{
			if (_sourceWeakNotifyCollectionChangedEventHandler != null)
			{
				_source.CollectionChanged -= _sourceWeakNotifyCollectionChangedEventHandler.Handle;			
			}

			if (_sourceScalarWeakPropertyChangedEventHandler != null)
			{
				_sourceScalar.PropertyChanged -= _sourceScalarWeakPropertyChangedEventHandler.Handle;			
			}

			if (_indexScalarWeakPropertyChangedEventHandler != null)
			{
				_indexValueScalar.PropertyChanged -= _indexScalarWeakPropertyChangedEventHandler.Handle;			
			}

			if (_defaultValueScalarWeakPropertyChangedEventHandler != null)
			{
				_defaultValueScalar.PropertyChanged -= _defaultValueScalarWeakPropertyChangedEventHandler.Handle;			
			}
		}

		public void ValidateConsistency()
		{
			IList<TSourceItem> source = _sourceScalar.getValue(_source, new ObservableCollection<TSourceItem>()) as IList<TSourceItem>;
			int index = _indexValueScalar.getValue(_index);
			TSourceItem defaultValue = _defaultValueScalar.getValue(_defaultValue);

			// ReSharper disable once PossibleNullReferenceException
			if (source.Count > index)
			{
				if (!source[index].IsSameAs(_value))
					throw new ObservableCalculationsException("Consistency violation: ItemCalculating.1");
			}
			else
			{
				if (!defaultValue.IsSameAs(_value))
					throw new ObservableCalculationsException("Consistency violation: ItemCalculating.2");			
			}
		}

	}
}
