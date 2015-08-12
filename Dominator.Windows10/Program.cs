using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dominator.Windows10.Tools;
using static Dominator.Windows10.Localization.Application;

namespace Dominator.Windows10
{
	static class Program
	{
		static readonly string ApplicationName = makeApplicationName();
		const string ProjectIssuesURL = "https://github.com/pragmatrix/Dominator/issues";

		[STAThread]
		public static int Main(string[] args)
		{
			try
			{
				if (!TryRunAsAdministrator())
					return 0;

				ProtectedMain(args);
				return 0;
			}
			catch (Exception e)
			{
				var text = string.Format(M_Sorry___0_crashed, ApplicationName, ProjectIssuesURL, e);
				MessageBox.Show(text, ApplicationName);
				return 5;
			}
		}

		static void ProtectedMain(string[] args)
		{
			if (args.Length != 0)
				throw new InvalidOperationException(M_No_support_for_command_lines_arguments_yet_);

			var app = new Application();

			var window = new Window
			{
				WindowStartupLocation = WindowStartupLocation.CenterScreen,
				Width = 640,
				Height = 480,
				Title = ApplicationName,
			};

			// optimize text quality on low resolution screens.
			if (DPI.Display < 125)
				TextOptions.SetTextFormattingMode(window, TextFormattingMode.Display);

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

		static bool TryRunAsAdministrator()
		{
			if (IsRunAsAdministrator())
				return true;

			var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase)
			{
				UseShellExecute = true,
				Verb = "runas"
			};

			try
			{
				Process.Start(processInfo);
				return false;
			}
			catch (Exception)
			{
				// The user did not allow the application to run as administrator
				MessageBox.Show(string.Format(M_Sorry___0__must_be_run_as_Administrator_, ApplicationName));
				return false;
			}
		}

		private static bool IsRunAsAdministrator()
		{
			var wi = WindowsIdentity.GetCurrent();
			var wp = new WindowsPrincipal(wi);

			return wp.IsInRole(WindowsBuiltInRole.Administrator);
		}

		static string makeApplicationName()
		{
			var assembly = typeof(Program).Assembly;
			var product = (AssemblyProductAttribute)(assembly.GetCustomAttributes(typeof(AssemblyProductAttribute)).First());
			return product.Product;
		}
	}
}
