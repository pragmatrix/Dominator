using Dominator.Net;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		public static GroupBuilder OptionalProtections(this GroupBuilder dsl) => dsl
			.BeginGroup("Optional Protections")
			.Explanation("Some of them are actually useful")
			
				.BeginItem("Turn on SmartScreen Filter to check web content (URLs) that Windows Store apps use")
				.RegistryUserValueWithHKLMDefault(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppHost", "EnableWebContentEvaluation", 0, 1)
				.End()

			.End();
	}
}
