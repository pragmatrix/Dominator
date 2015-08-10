using System;
using System.Linq;
using Dominator.Net;
using Dominator.Windows10.Tools;
using Microsoft.Win32;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		const string OptionNotFoundMessage = "Option not found. Evil defaults may apply.";
		const string ValueNotRecognizedMessage = "Option value is {0} and probably safe to change.";

		static ItemBuilder RegistryValue(this ItemBuilder dsl, string key, string valueName, uint dominatedValue, uint submissiveValue)
		{
			return dsl
				.Setter(
					action => Registry.SetValue(key, valueName, action == DominationAction.Dominate ? dominatedValue : submissiveValue, RegistryValueKind.DWord))
				.Getter(() =>
				{
					var value = Registry.GetValue(key, valueName, null);

					if (!(value is int))
						return DominatorState.Indetermined(OptionNotFoundMessage);
					
					var v = (uint)(int)value;
					if (v == dominatedValue)
						return DominatorState.Dominated();
					if (v == submissiveValue)
						return DominatorState.Submissive();
					return DominatorState.Indetermined(string.Format(ValueNotRecognizedMessage, v));
				});
		}

		static ItemBuilder RegistryValue(this ItemBuilder dsl, string key, string valueName, string dominatedValue, string submissiveValue)
		{
			return dsl
				.Setter(
					action => Registry.SetValue(key, valueName, action == DominationAction.Dominate ? dominatedValue : submissiveValue, RegistryValueKind.String))
				.Getter(() =>
				{
					var value = Registry.GetValue(key, valueName, null);

					if (!(value is string))
						return DominatorState.Indetermined(OptionNotFoundMessage);

					var v = (string)value;
					if (v == dominatedValue)
						return DominatorState.Dominated();
					if (v == submissiveValue)
						return DominatorState.Submissive();
					return DominatorState.Indetermined(string.Format(ValueNotRecognizedMessage, v));
				});
		}

		const string ServiceDoesNotExist = "Service is not installed.";
		const string ServiceStateNotRecognized = "Service status is not recognized, but safe to change.";

		public static ItemBuilder Service(this ItemBuilder dsl, string name, ServiceStartup dominate, ServiceStartup makeSubmissive)
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
						var configuration = ServiceTools.TryGetConfiguration(name);
						if (configuration == null)
							return (dominate == ServiceStartup.Disabled)
								? DominatorState.Dominated(ServiceDoesNotExist)
								: DominatorState.Indetermined(ServiceStateNotRecognized);

						if (configuration.Value.Startup == dominate)
							return DominatorState.Dominated();
						if (configuration.Value.Startup == makeSubmissive)
							return DominatorState.Submissive();

						return DominatorState.Indetermined("Service status is not recognized, but safe to change.");
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
						? DominatorState.Dominated()
						: DominatorState.Submissive();
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
