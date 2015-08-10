using Dominator.Net;
using Dominator.Windows10.Tools;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		public static GroupBuilder PrivacySettings(this GroupBuilder dsl) => dsl
			.BeginGroup("Privacy")
			.Explanation("Settings that protect your privacy")
				.BeginItem("Let apps use my advertising ID")
				.RegistryUserValueWithHKLMDefault(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, 1)
				.End()

				.BeginItem("Send Microsoft info about how I write")
				.RegistryUserValueWithHKLMDefault(@"SOFTWARE\Microsoft\Input\TIPC", "Enabled", 0, 1)
				.End()

				// no HKLM backing field, is on by default on Express Settings and Custom.
				.BeginItem("Let websites provide locally relevant content by accessing my language list")
				.RegistryValue(@"HKEY_CURRENT_USER\Control Panel\International\User Profile", "HttpAcceptLanguageOptOut", 1, 0, optionNotFound: DominatorState.Submissive())
				.End()

				.BeginItem("Send data about functional issues to Microsoft (Diagnostics Tracking Service)")
				.Service("DiagTrack", ServiceStartup.Disabled, ServiceStartup.Automatic)
				.End()

				.BeginItem("Ask for feedback")
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, 1, optionNotFound: DominatorState.Submissive())
				.End()

			/*
				.BeginItem("Diagnostics Tracking Log")
				.Explanation("Keep the log file about functional issues")
				.File(Environment.SpecialFolder.CommonApplicationData, @"Microsoft\Diagnosis\ETLLogs\AutoLogger\AutoLogger-Diagtrack-Listener.etl", FileConfiguration.MissingOrEmpty, FileConfiguration.ExistingAndNotEmpty)
				.End()
			*/

				.BeginItem("Log keystrokes (WAP Push Message Routing Service)")
				.Service("dmwappushservice", ServiceStartup.Disabled, ServiceStartup.Automatic)
				.End()

				.BeginGroup("Telemetry")
				.Explanation("Microsoft telemetry data collection")

					// Value is set to 3 in Express Settings and 2 in Custom Settings. Key always exists.
					.BeginItem("Collect telemetry data")
					.RegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0, 1, alsoTreatAsSubmissive: v => v >= 1 && v <=3)
					.End()

					.BeginItem("Allow this PC to connect to Microsoft telemetry servers")
					.Hosts("Settings/telemetry.txt")
					.End()
				.End()

				.BeginGroup("Location")

					.BeginItem("Allow apps and services to request your location")
					.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\DeviceAccess\Global\{BFA794E4-F964-4FDB-90F6-51056BFE4B44}", "Value", "Deny", "Allow")
					.End()
				.End()

				// Express Settings: the Search key exists in HKLM, but nothing is in there, and changing BingSearchEnabled does not have an
				// effect when the user key is not set.
				.BeginItem("Provide web results when I use the Windows search bar")
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0, 1, optionNotFound: DominatorState.Submissive())
				.End()

			.End();
	}
}
