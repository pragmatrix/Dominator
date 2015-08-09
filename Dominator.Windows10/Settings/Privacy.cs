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
				// on a fresh Windows 10 installation with Express Settings, the initial state is in Submissive, even thought the entry does not exist, hmm.
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, 1)
				.End()

				.BeginItem("Send Microsoft info about how I write")
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Input\TIPC", "Enabled", 0, 1)
				.End()

				.BeginItem("Let websites provide locally relevant content by accessing my language list")
				.RegistryValue(@"HKEY_CURRENT_USER\Control Panel\International\User Profile", "HttpAcceptLanguageOptOut", 1, 0)
				.End()

				.BeginItem("Send data about functional issues to Microsoft")
				.Service("DiagTrack", ServiceConfiguration.Disabled, new ServiceConfiguration(ServiceStartup.Automatic, ServiceStatus.Started) )
				.End()

				.BeginItem("Ask for feedback")
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, 1)
				.End()

			/*
				.BeginItem("Diagnostics Tracking Log")
				.Explanation("Keep the log file about functional issues empty")
				.File(Environment.SpecialFolder.CommonApplicationData, @"Microsoft\Diagnosis\ETLLogs\AutoLogger\AutoLogger-Diagtrack-Listener.etl", FileConfiguration.MissingOrEmpty, FileConfiguration.ExistingAndNotEmpty)
				.End()
			*/

				.BeginItem("Log keystrokes (WAP Push Message Routing Service)")
				.Service("dmwappushsvc", ServiceConfiguration.Disabled, new ServiceConfiguration(ServiceStartup.Automatic, ServiceStatus.Started) )
				.End()

				.BeginGroup("Telemetry")
				.Explanation("Microsoft telemetry data collection")

					.BeginItem("Collect telemetry data")
					.RegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0, 1)
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

				.BeginItem("Provide web results when I use the Windows search bar")
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0, 1)
				.End()

			.End();
	}
}
