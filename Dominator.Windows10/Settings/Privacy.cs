using System;
using Dominator.Net;
using Dominator.Windows10.Tools;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		public static GroupBuilder PrivacySettings(this GroupBuilder dsl) => dsl
			.BeginGroup("Privacy")
			.Explanation("Settings that protect your privacy")
				.BeginItem("Advertising ID")
				.Explanation("Do not let apps use my advertising ID")
				// on a fresh Windows 10 installation with Express Settings, the initial state is in Submissive, even thought the entry does not exist, hmm.
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, 1)
				.End()

				.BeginItem("Writing Tracking")
				.Explanation("Do not send Microsoft info about how I write")
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Input\TIPC", "Enabled", 0, 1)
				.End()

				.BeginItem("Languages")
				.Explanation("Do not let websites provide locally relevant content by accessing my language list")
				.RegistryValue(@"HKEY_CURRENT_USER\Control Panel\International\User Profile", "HttpAcceptLanguageOptOut", 1, 0)
				.End()

				.BeginItem("Diagnostics Tracking Service")
				.Explanation("Do not send data about functional issues to Microsoft")
				.Service("DiagTrack", ServiceConfiguration.Disabled, new ServiceConfiguration(ServiceStartup.Automatic, ServiceStatus.Started) )
				.End()

				.BeginItem("Feedback")
				.Explanation("Never ask for feedback")
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, 1)
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

				.BeginGroup("Telemetry")
				.Explanation("Microsoft telemetry data collection")

					.BeginItem("Telemetry Collection")	
					.Explanation("Do not collect telemetry data")
					.RegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0, 1)
					.End()

					.BeginItem("Telemetry URLs")
					.Explanation("Block all Microsoft telemetry URLs in the system's hosts file")
					.Hosts("Settings/telemetry.txt")
					.End()
				.End()

			.End();
	}
}
