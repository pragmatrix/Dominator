using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Dominator.Net;
using Microsoft.Win32;

namespace Dominator.Windows10
{
	static class Program
	{
		static readonly string ApplicationName = makeApplicationName();

		static string makeApplicationName()
		{
			var assembly = typeof (Program).Assembly;
			var product = (AssemblyProductAttribute) (assembly.GetCustomAttributes(typeof (AssemblyProductAttribute)).First());
			return product.Product;
		}

		[STAThread]
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
			var window = new Window
			{
				WindowStartupLocation = WindowStartupLocation.CenterScreen,
				Width = 640,
				Height = 480,
				Title = ApplicationName
			};

			var specification = BuildSpecification();

			using (var controller = new UIController())
			{
				var ui = UI.ForDominator(specification, controller);

				var container = new Grid
				{
					Margin = new Thickness(16)
				};
				container.Children.Add(ui);
				window.Content = container;

				app.Run(window);
			}
		}

		static IDominator BuildSpecification() => DSL
			.BeginGroup("Windows 10 Dominator")
			.Explanation("Manage all privacy related settings in one place.")
			.PrivacySettings()
			.Specification();

		static GroupBuilder PrivacySettings(this GroupBuilder dsl)
		{
			return dsl
				.BeginItem("Advertising ID")
				.Explanation("Don't let apps use my advertising ID")
				.RegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, 1)
				.End();
		}

		static GroupBuilder WiFiDefaults(this GroupBuilder dsl)
		{
			return dsl;
		}

		static GroupBuilder Cortana(this GroupBuilder dsl)
		{
			return dsl;
		}

		static ItemBuilder RegistryValue(this ItemBuilder dsl, string key, string valueName, uint dominatedValue, uint submissiveValue)
		{
			return dsl
				.Setter(
					action => Registry.SetValue(key, valueName, action == DominationAction.Dominate ? dominatedValue : submissiveValue, RegistryValueKind.DWord))
				.Getter(() =>
				{
					var value = Registry.GetValue(key, valueName, null);
					if (!(value is int))
						return DominatorState.Indetermined;
					var v = (uint)(int) value;
					if (v == dominatedValue)
						return DominatorState.Dominated;
					if (v == submissiveValue)
						return DominatorState.Submissive;
					return DominatorState.Indetermined;
				});
		}
	}
}
