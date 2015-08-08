using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		void scheduleDominationAndFeedback(IDominatorItem dominator, DominationAction action)
		{
			_dispatcher.QueueAction(() =>
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
			_dispatcher.QueueAction(() =>
			{
				var state = dominator.QueryState();
				scheduleToUI(() => feedBackState(dominator, state));
			});
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

		[Conditional("DEBUG")]
		void requireOnUIThread()
		{
			Debug.Assert(Dispatcher.CurrentDispatcher == _uiThreadDispatcher);
		}
	}
}