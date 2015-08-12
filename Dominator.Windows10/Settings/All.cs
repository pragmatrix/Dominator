using System.Windows;
using Dominator.Net;

namespace Dominator.Windows10.Settings
{
	using static Localization.Settings;

	static partial class Settings
	{
		public static IDominator All => DSL
			.BeginGroup("Windows 10 Dominator")
			.Explanation(E_Manage_all_privacy_related_settings_in_one_place)
			.PrivacySettings()
			.Annoyances()
			.OptionalProtections()
			.Specification();
	}
}
