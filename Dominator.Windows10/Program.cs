using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

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
				Title = ApplicationName,
			};

			var allSettings = Settings.Settings.All;

			using (var controller = new UIController())
			{
				var ui = UI.ForDominator(allSettings, controller);

				var container = new ScrollViewer
				{
					Padding = new Thickness(16),
					VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
					Content = ui
				};

				window.Deactivated += (sender, eventArgs) => ui.IsEnabled = false;
				window.Activated += (sender, eventArgs) =>
				{
					ui.IsEnabled = true;
					controller.scheduleUpdateAllStates();
				};

				window.Content = container;
				controller.scheduleUpdateAllStates();
				app.Run(window);
			}
		}
	}
}
