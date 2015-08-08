using System;

namespace Dominator.Net
{
	public enum DominatorState
	{
		Submissive,
		Dominated,
		Indetermined
	}

	public enum DominationAction
	{
		Dominate,
		MakeSubmissive
	}

	public struct DominatorDescription
	{
		public DominatorDescription(string title, string explanation)
		{
			Title = title;
			Explanation = explanation;
		}

		public readonly string Title;
		public readonly string Explanation;
	}

	public interface IDominator
	{
		DominatorDescription Description { get; }
	}

	public interface IDominatorItem : IDominator
	{
		DominatorState GetState();
		void SetState(DominationAction action);
	}

	public interface IDominatorGroup : IDominator
	{
		IDominator[] Nested { get; }
	}

	public enum DominatorClass
	{
		Group,
		Item
	}

	public static class DominatorExtensions
	{
		public static void DispatchTo(this IDominator dominator, Action<IDominatorGroup> groupHandler, Action<IDominatorItem> itemHandler)
		{
			var group_ = dominator as IDominatorGroup;
			if (group_ != null)
			{
				groupHandler(group_);
				return;
			}

			var item_ = dominator as IDominatorItem;
			if (item_ != null)
			{
				itemHandler(item_);
				return;
			}

			throw new InvalidOperationException(dominator.ToString());
		}

		public static R DispatchTo<R>(this IDominator dominator, Func<IDominatorGroup, R> groupFunction, Func<IDominatorItem, R> itemFunction)
		{
			var group_ = dominator as IDominatorGroup;
			if (group_ != null)
				return groupFunction(group_);

			var item_ = dominator as IDominatorItem;
			if (item_ != null)
				return itemFunction(item_);

			throw new InvalidOperationException(dominator.ToString());
		}

		public static DominatorClass classify(this IDominator dominator)
		{
			return dominator.DispatchTo(_ => DominatorClass.Group, _ => DominatorClass.Item);
		}
	}
}
