using System;
using System.Diagnostics;

namespace Dominator.Windows10.Tools
{
	static class Soft
	{
		public class Section
		{
			public bool IsLocked { get; private set; }

			public IDisposable Lock()
			{
				Debug.Assert(!IsLocked);
				IsLocked = true;
				return new DisposeAction(() =>
				{
					Debug.Assert(IsLocked);
					IsLocked = false;
				});
			}
		}
	}
}
