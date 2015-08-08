using System;
using Dominator.Net;
using Microsoft.Win32;
using System.ServiceProcess;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		static ItemBuilder RegistryValue(this ItemBuilder dsl, string key, string valueName, uint dominatedValue, uint submissiveValue, DominatorState entryMissingState = DominatorState.Indetermined)
		{
			return dsl
				.Setter(
					action => Registry.SetValue(key, valueName, action == DominationAction.Dominate ? dominatedValue : submissiveValue, RegistryValueKind.DWord))
				.Getter(() =>
				{
					var value = Registry.GetValue(key, valueName, null);
					if (!(value is int))
						return entryMissingState;
					var v = (uint)(int)value;
					if (v == dominatedValue)
						return DominatorState.Dominated;
					if (v == submissiveValue)
						return DominatorState.Submissive;
					return DominatorState.Indetermined;
				});
		}
	}
}
