using Dominator.Net;
using Dominator.Windows10.Tools;
using static Dominator.Windows10.Settings.Localization.Settings;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		public static GroupBuilder PrivacySettings(this GroupBuilder dsl) => dsl
			.BeginGroup(T_Privacy)
			.Explanation(E_Settings_that_protect_your_privacy)
				.BeginItem(E_Let_apps_use_my_advertising_ID)
				.RegistryUserValueWithHKLMDefault(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, 1)
				.End()

				.BeginItem(E_Send_Microsoft_info_about_how_I_write)
				.RegistryUserValueWithHKLMDefault(@"SOFTWARE\Microsoft\Input\TIPC", "Enabled", 0, 1)
				.End()

				// no HKLM backing field, is on by default on Express Settings and Custom.
				.BeginItem(E_Let_websites_provide_locally_relevant_content_by_accessing_my_language_list)
				.RegistryValue(@"HKEY_CURRENT_USER\Control Panel\International\User Profile", "HttpAcceptLanguageOptOut", 1, 0, optionNotFound: DominatorState.Submissive())
				.End()

				.BeginItem(E_Send_data_about_functional_issues_to_Microsoft)
				.Service("DiagTrack", ServiceStartup.Disabled, ServiceStartup.Automatic)
				.End()

				.BeginItem(E_Ask_for_feedback)
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, 1, optionNotFound: DominatorState.Submissive())
				.End()

			/*
				.BeginItem("Diagnostics Tracking Log")
				.Explanation("Keep the log file about functional issues")
				.File(Environment.SpecialFolder.CommonApplicationData, @"Microsoft\Diagnosis\ETLLogs\AutoLogger\AutoLogger-Diagtrack-Listener.etl", FileConfiguration.MissingOrEmpty, FileConfiguration.ExistingAndNotEmpty)
				.End()
			*/

				.BeginItem(E_Log_keystrokes)
				.Service("dmwappushservice", ServiceStartup.Disabled, ServiceStartup.Automatic)
				.End()

				.BeginGroup(T_Telemetry)
				.Explanation(E_Microsoft_telemetry_data_collection)

					// Value is set to 3 in Express Settings and 2 in Custom Settings. Key always exists.
					.BeginItem(E_Collect_telemetry_data)
					.RegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0, 1, alsoTreatAsSubmissive: v => v >= 1 && v <=3)
					.End()

					.BeginItem(E_Allow_this_PC_to_connect_to_Microsoft_telemetry_servers)
					.Hosts("Settings/telemetry.txt")
					.End()
				.End()

				.BeginGroup(T_Location)

					.BeginItem(E_Allow_apps_and_services_to_request_your_location)
					.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\DeviceAccess\Global\{BFA794E4-F964-4FDB-90F6-51056BFE4B44}", "Value", "Deny", "Allow")
					.End()
				.End()

				// Express Settings: the Search key exists in HKLM, but nothing is in there, and changing BingSearchEnabled does not have an
				// effect when the user key is not set.
				.BeginItem(E_Provide_web_results_when_I_use_the_Windows_search_bar)
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0, 1, optionNotFound: DominatorState.Submissive())
				.End()

			.End();
	}
}
