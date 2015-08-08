using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml.Schema;
using Dominator.Net;
using Microsoft.Win32;

namespace Dominator.Windows10.Settings
{
	static class ServiceTools
	{
		public static ItemBuilder Service(this ItemBuilder dsl, string name, ServiceConfiguration dominate, ServiceConfiguration makeSubmissive)
		{
			return dsl
				.Setter(
					action =>
					{
						Configure(name, action == DominationAction.Dominate ? dominate : makeSubmissive);
					})
				.Getter(
					() =>
					{
						var configuration = GetConfiguration(name);
						if (configuration == dominate)
							return DominatorState.Dominated;
						if (configuration == makeSubmissive)
							return DominatorState.Submissive;
						return DominatorState.Indetermined;
					});
		}

		static void Configure(string name, ServiceConfiguration config)
		{
			if (!IsExisting(name))
				return;

			SetServiceStartup(name, config.Startup);
			SetServiceStatus(name, config.Status);
		}

		static ServiceConfiguration GetConfiguration(string name)
		{
			if (!IsExisting(name))
				return new ServiceConfiguration(ServiceStartup.Disabled, ServiceStatus.Stopped);
			var startup = TryGetServiceStartup(name);
			if (startup == null)
				throw new Exception($"Service {name} exists, but Startup is not configured");
			var status = Status(name);
			return new ServiceConfiguration(startup.Value, ToServiceStatus(status));
		}

		static ServiceStatus ToServiceStatus(ServiceControllerStatus status)
		{
			switch (status)
			{
				case ServiceControllerStatus.StopPending:
					return ServiceStatus.Stopped;
				case ServiceControllerStatus.Stopped:
					return ServiceStatus.Stopped;
				default:
				return ServiceStatus.Started;
			}
		}

		public static bool IsExisting(string name)
		{
			return Registry.GetValue(mkServiceKey(name), "ImagePath", null) != null;
		}

		public static ServiceStartup? TryGetServiceStartup(string name)
		{
			var value = Registry.GetValue(mkServiceKey(name), "Start", null);
			if (!(value is int))
				return null;

			switch ((int)value)
			{
				case (int)ServiceStartup.Automatic:
				return ServiceStartup.Automatic;
				case (int)ServiceStartup.Manual:
				return ServiceStartup.Manual;
				case (int)ServiceStartup.Disabled:
				return ServiceStartup.Disabled;
			}

			return null;
		}

		public static void SetServiceStartup(string name, ServiceStartup startup)
		{
			Registry.SetValue(mkServiceKey(name), "Start", (int)startup, RegistryValueKind.DWord);
		}

		public static void SetServiceStatus(string name, ServiceStatus status)
		{
			switch (status)
			{
				case ServiceStatus.Started:
					Start(name, StatusChangeTimeout);
				return;
				case ServiceStatus.Stopped:
					Stop(name, StatusChangeTimeout);
				return;
			}

			Debug.Assert(false, status.ToString());
		}

		static readonly TimeSpan StatusChangeTimeout = TimeSpan.FromMilliseconds(5000);

		public static ServiceControllerStatus Status(string service)
		{
			using (var sc = new ServiceController(service))
			{
				return sc.Status;
			}
		}

		public static void Stop(string service, TimeSpan timeToWait)
		{
			using (var sc = new ServiceController(service))
			{
				if (sc.Status == ServiceControllerStatus.Stopped)
					return;
				sc.Stop();
				sc.WaitForStatus(ServiceControllerStatus.Stopped, timeToWait);
				System.Console.WriteLine(sc.Status);
				if (sc.Status != ServiceControllerStatus.Stopped)
					throw new Exception($"Failed to stop service {service}");
			}
		}

		public static void Start(string service, TimeSpan timeToWait)
		{
			using (var sc = new ServiceController(service))
			{
				if (sc.Status == ServiceControllerStatus.Running)
					return;
				sc.Start();
				sc.WaitForStatus(ServiceControllerStatus.Running, timeToWait);
				if (sc.Status != ServiceControllerStatus.Running)
					throw new Exception($"Failed to start service {service}");
			}
		}


		static string mkServiceKey(string service)
		{
			return BaseKey + "\\" + service;
		}

		const string BaseKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services";
	}
}
