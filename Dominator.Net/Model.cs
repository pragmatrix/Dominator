using System;

namespace Dominator.Net
{
	sealed class Item : IDominatorItem
	{
		internal Item(
			DominatorDescription description,
			Func<DominatorState> getter,
			Action<DominationAction> setter)
		{
			Description = description;
			_getter = getter;
			_setter = setter;
		}

		readonly Func<DominatorState> _getter;
		readonly Action<DominationAction> _setter;

		public DominatorDescription Description { get; }

		public DominatorState GetState()
		{
			return _getter();
		}

		public void SetState(DominationAction action)
		{
			_setter(action);
		}
	}

	sealed class Group : IDominatorGroup
	{
		public Group(DominatorDescription description, IDominator[] nested)
		{
			Description = description;
			Nested = nested;
		}

		public DominatorDescription Description { get; }

		public IDominator[] Nested { get; }
	}
}
