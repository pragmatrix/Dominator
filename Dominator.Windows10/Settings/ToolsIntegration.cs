using System;
using System.Linq;
using Dominator.Net;
using Dominator.Windows10.Tools;
using Microsoft.Win32;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		const string QueryFailedErrorMessage = "Query failed. Evil defaults may apply.";

		static ItemBuilder RegistryValue(this ItemBuilder dsl, string key, string valueName, uint dominatedValue, uint submissiveValue, DominatorState? entryMissingState = null)
		{
			return dsl
				.Setter(
					action => Registry.SetValue(key, valueName, action == DominationAction.Dominate ? dominatedValue : submissiveValue, RegistryValueKind.DWord))
				.Getter(() =>
				{
					var value = Registry.GetValue(key, valueName, null);

					if (!(value is int))
						if (entryMissingState != null)
							return entryMissingState.Value;
						else
							throw new Exception(QueryFailedErrorMessage);

					var v = (uint)(int)value;
					if (v == dominatedValue)
						return DominatorState.Dominated;
					if (v == submissiveValue)
						return DominatorState.Submissive;
					return DominatorState.Indetermined;
				});
		}

		static ItemBuilder RegistryValue(this ItemBuilder dsl, string key, string valueName, string dominatedValue, string submissiveValue, DominatorState? entryMissingState = null)
		{
			return dsl
				.Setter(
					action => Registry.SetValue(key, valueName, action == DominationAction.Dominate ? dominatedValue : submissiveValue, RegistryValueKind.String))
				.Getter(() =>
				{
					var value = Registry.GetValue(key, valueName, null);

					if (!(value is string))
						if (entryMissingState != null)
							return entryMissingState.Value;
						else
							throw new Exception(QueryFailedErrorMessage);

					var v = (string)value;
					if (v == dominatedValue)
						return DominatorState.Dominated;
					if (v == submissiveValue)
						return DominatorState.Submissive;
					return DominatorState.Indetermined;
				});
		}



		public static ItemBuilder Service(this ItemBuilder dsl, string name, ServiceConfiguration dominate, ServiceConfiguration makeSubmissive)
		{
			return dsl
				.Setter(
					action =>
					{
						ServiceTools.Configure(name, action == DominationAction.Dominate ? dominate : makeSubmissive);
					})
				.Getter(
					() =>
					{
						var configuration = ServiceTools.GetConfiguration(name);
						if (configuration == dominate)
							return DominatorState.Dominated;
						if (configuration == makeSubmissive)
							return DominatorState.Submissive;
						return DominatorState.Indetermined;
					});
		}

		public static ItemBuilder Hosts(this ItemBuilder dsl, string blockingFile)
		{
			return dsl
				.Getter(() =>
				{
					var hosts = HostsTools.ReadSystemHostsFile().SafeParseHostLines();
					var blocked = HostsTools.ReadHostsFile(blockingFile).SafeParseHostLines().ExtractEntries();
					return hosts.ContainsAllHostEntries(blocked) 
						? DominatorState.Dominated 
						: DominatorState.Submissive;
				})
				.Setter(action =>
				{
					var hosts = HostsTools.ReadSystemHostsFile().SafeParseHostLines();
					var blocked = HostsTools.ReadHostsFile(blockingFile).SafeParseHostLines().ExtractEntries();
					switch (action)
					{
						case DominationAction.Dominate:
						{
							var result = hosts.Merge(blocked).ToLines();
							HostsTools.WriteSystemHostsFile(result);
						}
						break;

						case DominationAction.MakeSubmissive:
						{
							var result = hosts.FilterHosts(blocked.Select(e => e.Host)).ToLines();
							HostsTools.WriteSystemHostsFile(result);
						}
						break;
					}
				});
		}
	}
}
