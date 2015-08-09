using Dominator.Net;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		public static GroupBuilder OptionalProtections(this GroupBuilder dsl) => dsl
			.BeginGroup("Optional Protections")
			.Explanation("Some of them are actually useful")
			
				.BeginItem("SmartScreen Filter")
				.Explanation("Turn off SmartScreen Filter to check web content (URLs) that Windows Store apps use")
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\AppHost", "EnableWebContentEvaluation", 0, 1, entryMissingState : DominatorState.Dominated)
				.End()

			.End();
	}
}
