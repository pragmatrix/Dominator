﻿using Dominator.Net;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		public static GroupBuilder Annoyances(this GroupBuilder dsl) => dsl
			.BeginGroup("Annoyances")
			.Explanation("Settings that may cause annoying consequences")

				.BeginItem("Show Skype home and advertisements")
				.Hosts("Settings/skype-ads.txt")
				.End()

				// https://techjourney.net/enable-or-disable-peer-to-peer-p2p-apps-updates-download-from-more-than-one-place-in-windows-10/

				// this registry key is not set with express settings, and but delivery optimization is enabled.
				.BeginItem("Get updates from or send updates to other PCs")
				.RegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config", "DODownloadMode", 0, 3)
				.End()

			.End();
	}
}
