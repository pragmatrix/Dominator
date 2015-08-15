using Dominator.Net;
using static Dominator.Windows10.Settings.Localization.Settings;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		public static GroupBuilder Annoyances(this GroupBuilder dsl) => dsl
			.BeginGroup(T_Annoyances)
			.Explanation(E_Settings_that_may_cause_annoying_consequences)

				.BeginItem(E_Show_Skype_home_and_advertisements)
				.Hosts("Settings/skype-ads.txt")
				.End()

				// https://techjourney.net/enable-or-disable-peer-to-peer-p2p-apps-updates-download-from-more-than-one-place-in-windows-10/

				.BeginItem(E_Get_updates_from_or_send_updates_to_other_PCs)
				.RegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config", "DODownloadMode", 0, 3, optionNotFound: DominatorState.Submissive())
				.MoreInSettings("windowsupdate")
				.End()

			.End();
	}
}
