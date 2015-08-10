using System;
using System.Collections.Generic;
using System.Linq;

namespace Dominator.Net
{
	public struct DominationState
	{
		public DominationState(DominatorState itemState)
			: this(itemState, null, Enumerable.Empty<DominationState>())
		{
		}

		public DominationState(Exception error)
			: this(DominatorState.Indetermined(""), error, Enumerable.Empty<DominationState>())
		{}

		public DominationState(DominatorState groupState, IEnumerable<DominationState> nested)
			: this(groupState, null, nested)
		{}

		DominationState(DominatorState? state, Exception error_, IEnumerable<DominationState> nested)
		{
			State_ = state;
			Error_ = error_;
			Nested = nested;
		}

		public readonly DominatorState? State_;
		public readonly Exception Error_;
		public readonly IEnumerable<DominationState> Nested;
	}

	public static class DominatorStateExtensions
	{
		public static DominationState QueryState(this IDominator dominator)
		{
			var item_ = dominator as IDominatorItem;
			if (item_ != null)
				return QueryItemState(item_);

			var group_ = dominator as IDominatorGroup;
			if (group_ != null)
				return QueryGroupState(group_);

			throw new InvalidOperationException(dominator.ToString());
		}

		static DominationState QueryItemState(IDominatorItem item)
		{
			try
			{
				var state = item.GetState();
				return new DominationState(state);
			}
			catch (Exception e)
			{
				return new DominationState(e);
			}
		}

		static DominationState QueryGroupState(IDominatorGroup group)
		{
			var nested = group.Nested.Select(QueryState).ToArray();
			var state = CumulativeState(nested.Select(ns => ns.State_));
			return new DominationState(state, nested);
		}

		public static DominatorState CumulativeState(this IEnumerable<DominatorState?> states)
		{
			var allDominated = states.All(state_ => state_ != null && state_.Value.Kind == DominatorStateKind.Dominated);
			if (allDominated)
				return DominatorState.Dominated();

			var allSubmissive = states.All(state_ => state_ != null && state_.Value.Kind == DominatorStateKind.Submissive);
			if (allSubmissive)
				return DominatorState.Submissive();

			return DominatorState.Indetermined("");
		}
	}
}
