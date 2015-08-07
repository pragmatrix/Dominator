using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Dominator.Net;

namespace Dominator.Windows10
{
	static class Program
	{
		public static int Main(string[] args)
		{
			try
			{
				ProtectedMain(args);
				return 0;
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);
				return 5;
			}
		}

		static void ProtectedMain(string[] args)
		{
			if (args.Length != 0)
				throw new InvalidOperationException("no support for arguments yet!");

			var app = new Application();
			var window = new Window();
			var specification = BuildSpecification();

			populateUI(window, specification);
			app.Run(window);
		}

		static IDominator BuildSpecification() => DSL
			.BeginGroup("Windows 10 Dominator")
			.Explanation("Manage all privacy settings in one place.")
			.PrivacySettings()
			.Specification();

		static GroupBuilder PrivacySettings(this GroupBuilder dsl)
		{
			return dsl;
		}

		static GroupBuilder WiFiDefaults(this GroupBuilder dsl)
		{
			return dsl;
		}

		static GroupBuilder Cortana(this GroupBuilder dsl)
		{
			return dsl;
		}
	}
}
