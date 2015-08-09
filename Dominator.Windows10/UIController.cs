using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using Dominator.Net;
using Dominator.Windows10.Tools;

namespace Dominator.Windows10
{
	sealed class UIController : IUIRegistrationContext, IDisposable
	{
		readonly Dispatcher _uiThreadDispatcher;
		readonly Dictionary<IDominatorItem, Action<DominationState>> _feedback = new Dictionary<IDominatorItem, Action<DominationState>>();
		readonly DedicatedThreadDispatcher _dispatcher = new DedicatedThreadDispatcher();
		readonly Dictionary<IDominatorItem, DominationState> _stateCache = new Dictionary<IDominatorItem, DominationState>(); 

		public UIController()
		{
			_uiThreadDispatcher = Dispatcher.CurrentDispatcher;
		}

		public void Dispose()
		{
			requireOnUIThread();

			_dispatcher.Dispose();
		}

		public void requestAction(IDominatorItem dominator, DominationAction action)
		{
			requireOnUIThread();
			scheduleDominationAndFeedback(dominator, action);
		}

		public void registerFeedback(IDominatorItem dominator, Action<DominationState> feedbackFunction)
		{
			requireOnUIThread();
			_feedback[dominator] = feedbackFunction;
		}

		public void scheduleUpdateAllStates()
		{
			var allItems = AllItems.ToArray();
			schedule(() =>
			{
				var allStates = allItems
					.Select(item => new { Item = item, State = item.QueryState() })
					.ToArray();

				scheduleToUI(() =>
				{
					foreach (var state in allStates)
						feedBackState(state.Item, state.State);
				});
			});
		}

		void scheduleDominationAndFeedback(IDominatorItem dominator, DominationAction action)
		{
			schedule(() =>
			{
				try
				{
					dominator.SetState(action);
					var state = dominator.QueryState();
					scheduleToUI(() => feedBackState(dominator, state));
				}
				catch (Exception e)
				{
					var state = new DominationState(e);
					scheduleToUI(() => feedBackState(dominator, state));
				}
			});
		}
	
		void scheduleFeedbackFor(IDominatorItem dominator)
		{
			schedule(() =>
			{
				var state = dominator.QueryState();
				scheduleToUI(() => feedBackState(dominator, state));
			});
		}

		void schedule(Action action)
		{
			_dispatcher.QueueAction(action);
		}

		void feedBackState(IDominatorItem dominator, DominationState state)
		{
			requireOnUIThread();
			_feedback[dominator](state);
		}

		void scheduleToUI(Action action)
		{
			_uiThreadDispatcher.InvokeAsync(action);
		}

		IEnumerable<IDominatorItem> AllItems
		{
			get
			{
				requireOnUIThread();
				return _feedback.Keys;
			}
		}

		[Conditional("DEBUG")]
		void requireOnUIThread()
		{
			Debug.Assert(Dispatcher.CurrentDispatcher == _uiThreadDispatcher);
		}
	}
}