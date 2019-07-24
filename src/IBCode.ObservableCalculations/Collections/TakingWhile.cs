﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using IBCode.ObservableCalculations.Common;
using IBCode.ObservableCalculations.Common.Interface;

namespace IBCode.ObservableCalculations
{
	public class TakingWhile<TSourceItem> : Selecting<ZipPair<int, TSourceItem>, TSourceItem>, IHasSources
	{
		public new IReadScalar<INotifyCollectionChanged> SourceScalar => _sourceScalarTakingWhile;

		// ReSharper disable once MemberCanBePrivate.Global
		public new INotifyCollectionChanged Source => _sourceTakingWhile;

		// ReSharper disable once MemberCanBePrivate.Global
		public Expression<Func<TSourceItem, int, bool>> PredicateExpression => _predicateExpression;

		public new ReadOnlyCollection<INotifyCollectionChanged> SourcesCollection => new ReadOnlyCollection<INotifyCollectionChanged>(new []{Source});
		public new ReadOnlyCollection<IReadScalar<INotifyCollectionChanged>> SourceScalarsCollection => new ReadOnlyCollection<IReadScalar<INotifyCollectionChanged>>(new []{SourceScalar});

		private readonly IReadScalar<INotifyCollectionChanged> _sourceScalarTakingWhile;
		private readonly INotifyCollectionChanged _sourceTakingWhile;
		private readonly Expression<Func<TSourceItem, int, bool>> _predicateExpression;

		// ReSharper disable once MemberCanBePrivate.Global

		[ObservableCalculationsCall]
		public TakingWhile(			
			IReadScalar<INotifyCollectionChanged> sourceScalar, 
			Expression<Func<TSourceItem, int, bool>> predicateExpression,
			int capacity = 0)
			: base(
				getSource(sourceScalar, predicateExpression, capacity),
				zipPair => zipPair.ItemRight)
		{
			_sourceScalarTakingWhile = sourceScalar;
			_predicateExpression = predicateExpression;
		}

		[ObservableCalculationsCall]
		public TakingWhile(			
			INotifyCollectionChanged source, 
			Expression<Func<TSourceItem, int, bool>> predicateExpression,
			int capacity = 0)
			: base(
				getSource(source, predicateExpression, capacity),
				zipPair => zipPair.ItemRight)
		{
			_sourceTakingWhile = source;
			_predicateExpression = predicateExpression;
		}

		[ObservableCalculationsCall]
		public TakingWhile(
			IReadScalar<INotifyCollectionChanged> sourceScalar, 
			Expression<Func<TSourceItem, bool>> predicateExpression,
			int capacity = 0) : this(sourceScalar, predicateExpression.getIndexedPredicate(), capacity)
		{
		}

		[ObservableCalculationsCall]
		public TakingWhile(
			INotifyCollectionChanged source,
			Expression<Func<TSourceItem, bool>> predicateExpression,
			int capacity = 0) : this(source, predicateExpression.getIndexedPredicate(), capacity)
		{
		}

		private static INotifyCollectionChanged getSource(
			IReadScalar<INotifyCollectionChanged> sourceScalar, 
			Expression<Func<TSourceItem, int, bool>> predicateExpression,
			int capacity)
		{
			Expression<Func<ZipPair<int, TSourceItem>, bool>> zipPairNotPredicateExpression = getZipPairNotPredicateExpression(predicateExpression);

			Calculating<int> countCalculating = Expr.Is(() => sourceScalar.Value != null ? ((IList) sourceScalar.Value).Count : 0).Calculating();

			Zipping<int, TSourceItem> zipping = countCalculating.SequenceCalculating()
				.Zipping<int, TSourceItem>(sourceScalar);

			return getFiltering(zipping, zipPairNotPredicateExpression, countCalculating, capacity);

			//return () => (INotifyCollectionChanged)Expr.Is(() => (INotifyCollectionChanged)getSource.Calculating().Using(sc =>
			//			Expr.Is(() => ((IList) sc.Value).Count).SequenceCalculating()
			//				.Zipping<int, TSourceItem>(() => sc.Value)).Value).Calculating().Using(zipping => zipping.Value.Filtering<ZipPair<int, TSourceItem>>(zp => zp.ItemLeft < zipping.Value.Filtering(zipPairNotPredicateExpression).Selecting(zp1 => zp1.ItemLeft).Minimazing(() => (((IList)zipping.Value).Count)).Value)).Value;
		}

		private static Filtering<ZipPair<int, TSourceItem>> getFiltering(
			Zipping<int, TSourceItem> zipping, 
			Expression<Func<ZipPair<int, TSourceItem>, bool>> zipPairNotPredicateExpression, 
			Calculating<int> countCalculating,
			int capacity)
		{
			return zipping.Filtering(zp => zp.ItemLeft < zipping.Filtering(zipPairNotPredicateExpression, capacity).Selecting(zp1 => zp1.ItemLeft).Minimazing<int>(countCalculating).Value, capacity);
		}

		private static INotifyCollectionChanged getSource(
			INotifyCollectionChanged source, 
			Expression<Func<TSourceItem, int, bool>> predicateExpression,
			int capacity)
		{
			Expression<Func<ZipPair<int, TSourceItem>, bool>> zipPairNotPredicateExpression = getZipPairNotPredicateExpression(predicateExpression);

			Calculating<int> countCalculating = Expr.Is(() => ((IList)source).Count).Calculating();

			Zipping<int, TSourceItem> zipping = countCalculating.SequenceCalculating()
				.Zipping<int, TSourceItem>(source);

			return getFiltering(zipping, zipPairNotPredicateExpression, countCalculating, capacity);
		}

		private static Expression<Func<ZipPair<int, TSourceItem>, bool>> getZipPairNotPredicateExpression(Expression<Func<TSourceItem, int, bool>> predicateExpression)
		{
			ParameterExpression zipPairParameterExpression
				= Expression.Parameter(typeof(ZipPair<int, TSourceItem>), "zipPair");
			Expression zipPairIndexExpression
				= Expression.PropertyOrField(
					zipPairParameterExpression,
					nameof(ZipPair<int, TSourceItem>.ItemLeft));
			Expression zipPairItemExpression
				= Expression.PropertyOrField(
					zipPairParameterExpression,
					nameof(ZipPair<int, TSourceItem>.ItemRight));
			ReplaceParameterVisitor replaceParameterVisitor
				= new ReplaceParameterVisitor(
					predicateExpression.Parameters,
					new[] {zipPairItemExpression, zipPairIndexExpression});
			Expression<Func<ZipPair<int, TSourceItem>, bool>> zipPairNotPredicateExpression
				= Expression.Lambda<Func<ZipPair<int, TSourceItem>, bool>>(
					// ReSharper disable once AssignNullToNotNullAttribute
					Expression.Not(replaceParameterVisitor.Visit(predicateExpression.Body)),
					zipPairParameterExpression);
			return zipPairNotPredicateExpression;
		}

		public new void ValidateConsistency()
		{
			IList<TSourceItem> source = _sourceScalarTakingWhile.getValue(_sourceTakingWhile, new ObservableCollection<TSourceItem>()) as IList<TSourceItem>;
			Func<TSourceItem, int, bool> predicate = _predicateExpression.Compile();

			// ReSharper disable once AssignNullToNotNullAttribute
			if (!this.SequenceEqual(source.TakeWhile(predicate)))
			{
				throw new ObservableCalculationsException("Consistency violation: TakingWhile.1");
			}
		}
	}
}
