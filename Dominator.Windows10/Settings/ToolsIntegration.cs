using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dominator.Net;
using Dominator.Windows10.Tools;
using Microsoft.Win32;
using static Dominator.Windows10.Settings.Localization.Settings;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		static readonly string OptionNotFoundMessage = M_Option_not_found__Evil_defaults_may_apply;
		static readonly string ValueNotRecognizedMessage = M_Option_value_is__0__and_probably_safe_to_change;

		static ItemBuilder MoreInSettings(this ItemBuilder dsl, string setting)
		{
			return dsl.MoreAt(new Uri($"ms-settings:{setting}", UriKind.Absolute));
		}

		static ItemBuilder MoreAt(this ItemBuilder dsl, Uri uri)
		{
			return dsl.More(() =>
			{
				Process.Start(uri.ToString());
			}, uri.ToString());
		}

		static ItemBuilder WarnWhenDominated(this ItemBuilder dsl, string warning)
		{
			return dsl.ChainGetter(state => state.Kind != DominatorStateKind.Dominated ? state : state.WithMessage(warning));
		}

		static ItemBuilder RegistryValue(this ItemBuilder dsl, string key, string valueName, int dominatedValue, int submissiveValue, DominatorState? optionNotFound = null, Func<int, bool> alsoTreatAsSubmissive = null)
		{
			Func<int, bool> isTreatedAsSubmissive = v => v == submissiveValue || (alsoTreatAsSubmissive != null && alsoTreatAsSubmissive(v));
				
			return dsl
				.Setter(
					action => Registry.SetValue(key, valueName, action == DominationAction.Dominate ? dominatedValue : submissiveValue, RegistryValueKind.DWord))
				.Getter(() =>
				{
					var value = Registry.GetValue(key, valueName, null);
					if (!(value is int))
						return optionNotFound ?? DominatorState.Indetermined(OptionNotFoundMessage);

					var v = (int)value;
					if (v == dominatedValue)
						return DominatorState.Dominated();
					if (isTreatedAsSubmissive(v))
						return DominatorState.Submissive();
					return DominatorState.Indetermined(string.Format(ValueNotRecognizedMessage, v));
				});
		}

		const string HKLM = "HKEY_LOCAL_MACHINE";
		const string HKCU = "HKEY_CURRENT_USER";

		static ItemBuilder RegistryUserValueWithHKLMDefault(this ItemBuilder dsl, string key, string valueName, int dominatedValue, int submissiveValue)
		{
			return dsl
				.Setter(
					action => Registry.SetValue(userKey(key), valueName, action == DominationAction.Dominate ? dominatedValue : submissiveValue, RegistryValueKind.DWord))
				.Getter(() =>
				{
					var value = tryGetUserOrMachineValue(key, valueName);
					if (!(value is int))
						return DominatorState.Indetermined(OptionNotFoundMessage);

					var v = (int)value;
					if (v == dominatedValue)
						return DominatorState.Dominated();
					if (v == submissiveValue)
						return DominatorState.Submissive();
					return DominatorState.Indetermined(string.Format(ValueNotRecognizedMessage, v));
				});
		}

		static object tryGetUserOrMachineValue(string key, string valueName)
		{
			var userValue = Registry.GetValue(userKey(key), valueName, null);
			return userValue ?? Registry.GetValue(machineKey(key), valueName, null);
		}

		static string machineKey(string key) => HKLM + @"\" + key;
		static string userKey(string key) => HKCU + @"\" + key;

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

		static readonly string ServiceDoesNotExistMessage = M_Service_is_not_installed;
		static readonly string ServiceStateNotRecognizedMessage = M_Service_status_is_not_recognized__but_safe_to_change;

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
								? DominatorState.Dominated(ServiceDoesNotExistMessage)
								: DominatorState.Indetermined(ServiceStateNotRecognizedMessage);

						if (configuration.Value.Startup == dominate)
							return DominatorState.Dominated();
						if (configuration.Value.Startup == makeSubmissive)
							return DominatorState.Submissive();

						return DominatorState.Indetermined(ServiceStateNotRecognizedMessage);
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
				})
				.More(showHostsFile, M_Show_the_system_s_hosts_file_);
		}

		static void showHostsFile()
		{
			var fn = Path.GetTempFileName();
			var fntxt = Path.ChangeExtension(fn, "txt");
			File.Move(fn, fntxt);
			File.Copy(HostsTools.SystemHostsFilePath, fntxt, overwrite: true);
			Process.Start(fntxt);
		}
	}
}
