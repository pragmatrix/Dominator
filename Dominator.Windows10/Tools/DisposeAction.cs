using System;

namespace Dominator.Windows10.Tools
{
	struct DisposeAction : IDisposable
	{
		readonly Action _action;

		public DisposeAction(Action action)
		{
			_action = action;
		}

		public void Dispose()
		{
			_action();
		}
	}
}
