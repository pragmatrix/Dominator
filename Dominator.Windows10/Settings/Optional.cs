using Dominator.Net;
using static Dominator.Windows10.Settings.Localization.Settings;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{

		public static GroupBuilder OptionalProtections(this GroupBuilder dsl) => dsl
			.BeginGroup(T_Optional_Protections)
			.Explanation(E_Some_of_them_are_actually_useful)
			
				.BeginItem(E_Turn_on_SmartScreen_Filter_to_check_web_content__URLs__that_Windows_Store_apps_use)
				.RegistryUserValueWithHKLMDefault(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppHost", "EnableWebContentEvaluation", 0, 1)
				.MoreInSettings("privacy")
				.End()

			.End();
	}
}
