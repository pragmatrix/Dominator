using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominator.Net;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		public static IDominator All => DSL
			.BeginGroup("Windows 10 Dominator")
			.Explanation("Manage all privacy related settings in one place")
			.PrivacySettings()
			.OptionalProtections()
			.Specification();
	}
}
