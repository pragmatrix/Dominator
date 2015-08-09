using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominator.Net;
using Dominator.Windows10.Tools;

namespace Dominator.Windows10.Settings
{
	static partial class Settings
	{
		public static GroupBuilder Annoyances(this GroupBuilder dsl) => dsl
			.BeginGroup("Annoyances")
			.Explanation("Settings that are very annoying")
				.BeginItem("Skype Home and Advertising")
				.Explanation("Block all Skype home and advertising URLs in the system's hosts file")
				.Hosts("Settings/skype-ads.txt")
				.End()
			.End();
	}
}
