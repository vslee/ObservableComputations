﻿using System.Collections.Generic;
using System.Collections.Specialized;

namespace IBCode.ObservableCalculations.Common.Interface
{
	public interface IOrdering<TSourceItem> : IList<TSourceItem>, INotifyCollectionChanged
	{
	}

	internal interface IOrderingInternal<TSourceItem> : IOrdering<TSourceItem>
	{
		RangePosition GetRangePosition(int orderedIndex);
		RangePositions<RangePosition> GetRangePositions();
	}

	public interface IThenOrdering<TSourceItem> : IList<TSourceItem>, INotifyCollectionChanged
	{
	}

	internal interface IThenOrderingInternal<TSourceItem> : IThenOrdering<TSourceItem>
	{
		void ProcessSourceItemChange(int sourceIndex);
	}
}
