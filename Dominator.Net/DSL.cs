using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dominator.Net
{
	public static class DSL
	{
		public static GroupBuilder BeginGroup(string title)
		{
			return new GroupBuilder(null, title);
		}
	}

	public class GroupBuilder
	{
		public GroupBuilder(GroupBuilder parent_, string title)
		{
			_parent_ = parent_;
			_title = title;
		}

		readonly GroupBuilder _parent_;
		readonly string _title;
		string _explanation_;
		readonly List<IDominator> _nested = new List<IDominator>();

		public ItemBuilder BeginItem(string title)
		{
			return new ItemBuilder(this, title);
		}

		public GroupBuilder BeginGroup(string title)
		{
			return new GroupBuilder(this, title);
		}

		public GroupBuilder Explanation(string explanation)
		{
			_explanation_ = explanation;
			return this;
		}

		public GroupBuilder End()
		{
			if (_parent_ == null)
				throw new InvalidOperationException($"{_title}: can't end root");
			_parent_.AddNested(Specification());
			return _parent_;
		}

		public IDominator Specification()
		{
			// it comes in handy to allow zero nested items when building new dominators.
			// if (_nested.Count == 0)
			//	throw new InvalidOperationException($"{_title}: no nested dominators");

			var description = new DominatorDescription(_title, _explanation_ ?? "");

			var dominator = new Group(description, _nested.ToArray());
			return dominator;
		}

		internal void AddNested(IDominator dominator)
		{
			_nested.Add(dominator);
		}
	}

	public class ItemBuilder
	{
		internal ItemBuilder(GroupBuilder parent, string title)
		{
			_parent = parent;
			_title = title;
		}

		readonly GroupBuilder _parent;
		readonly string _title;
		string _explanation_;
		More? _more_;

		Action<DominationAction> _setter_;
		Func<DominatorState> _getter_;

		public ItemBuilder Explanation(string explanation)
		{
			_explanation_ = explanation;
			return this;
		}

		public ItemBuilder More(Action action, string info = "")
		{
			_more_ = new More(action, info);
			return this;
		}

		public ItemBuilder Setter(Action<DominationAction> setter)
		{
			_setter_ = setter;
			return this;
		}

		public ItemBuilder Getter(Func<DominatorState> getter)
		{
			Debug.Assert(_getter_ == null, "Getter can not set twice, use ChainGetter instead");
			_getter_ = getter;
			return this;
		}

		public ItemBuilder ChainGetter(Func<DominatorState, DominatorState> getter)
		{
			Debug.Assert(_getter_ != null, "There must be a Getter() registered, before a call to ChainGetter()");

			var previous = _getter_;
			_getter_ = () =>
			{
				var state = previous();
				return getter(state);
			};
			return this;
		}
		
		public GroupBuilder End()
		{
			var dominator = Specification();
			_parent.AddNested(dominator);
			return _parent;
		}

		IDominator Specification()
		{
			if (_getter_ == null)
				throw new InvalidOperationException($"{_title}: missing getter");
			if (_setter_ == null)
				throw new InvalidOperationException($"{_title}: missing setter");

			var description = new DominatorDescription(_title, _explanation_ ?? "", _more_);
			var dominator = new Item(description, _getter_, _setter_);
			return dominator;
		}
	}
}
