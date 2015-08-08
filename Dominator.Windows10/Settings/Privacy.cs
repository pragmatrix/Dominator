using System;
using Dominator.Net;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		public static GroupBuilder PrivacySettings(this GroupBuilder dsl) => dsl
			.BeginGroup("Privacy")
			.Explanation("Settings that protect your privacy")
				.BeginItem("Advertising ID")
				.Explanation("Do not let apps use my advertising ID")
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, 1, DominatorState.Dominated)
				.End()

				.BeginItem("Languages")
				.Explanation("Do not let websites provide locally relevant content by accessing my language list")
				.RegistryValue(@"HKEY_CURRENT_USER\Control Panel\International\User Profile", "HttpAcceptLanguageOptOut", 1, 0)
				.End()
			/*
				.BeginItem("Telemetry")
				.Explanation("Disable all Microsoft telemetry service")
				.End()
			*/

				.BeginItem("Diagnostics Tracking Service")
				.Explanation("Do not send data about functional issues to Microsoft")
				.Service("DiagTrack", ServiceConfiguration.Disabled, new ServiceConfiguration(ServiceStartup.Automatic, ServiceStatus.Started) )
				.End()

			/*
				.BeginItem("Diagnostics Tracking Log")
				.Explanation("Keep the log file about functional issues empty")
				.File(Environment.SpecialFolder.CommonApplicationData, @"Microsoft\Diagnosis\ETLLogs\AutoLogger\AutoLogger-Diagtrack-Listener.etl", FileConfiguration.MissingOrEmpty, FileConfiguration.ExistingAndNotEmpty)
				.End()
			*/

				.BeginItem("WAP Push Message Routing Service")
				.Explanation("Do not log keystrokes")
				.Service("dmwappushsvc", ServiceConfiguration.Disabled, new ServiceConfiguration(ServiceStartup.Automatic, ServiceStatus.Started) )
				.End()


			.End();
	}
}
