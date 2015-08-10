using System;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;

namespace Dominator.Windows10.Tools
{
	static class ServiceTools
	{
		public static void Configure(string name, ServiceStartup startup)
		{
			if (!IsInstalled(name))
				throw new Exception("Service {name} is not installed. ");

			if (startup == ServiceStartup.Disabled)
				SetServiceStatus(name, ServiceStatus.Stopped);

			SetServiceStartup(name, startup);

			if (startup == ServiceStartup.Automatic)
				SetServiceStatus(name, ServiceStatus.Started);
		}

		public static ServiceConfiguration? TryGetConfiguration(string name)
		{
			if (!IsInstalled(name))
				return null;
			var startup = TryGetServiceStartup(name);
			if (startup == null)
				throw new Exception($"Service {name} is installed, but its startup option is not configured.");
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

		public static bool IsInstalled(string name)
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
				Console.WriteLine(sc.Status);
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
