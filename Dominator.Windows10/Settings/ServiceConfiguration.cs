using System.Windows.Media;

namespace Dominator.Windows10.Settings
{
	public enum ServiceStatus
	{
		Started,
		Stopped
	}

	public enum ServiceStartup
	{
		Automatic = 2,
		Manual = 3,
		Disabled = 4
	}

	public struct ServiceConfiguration
	{
		public readonly ServiceStartup Startup;
		public readonly ServiceStatus Status;

		public static readonly ServiceConfiguration Disabled = new ServiceConfiguration(ServiceStartup.Disabled, ServiceStatus.Stopped);

		public ServiceConfiguration(ServiceStartup startup, ServiceStatus status)
		{
			Startup = startup;
			Status = status;
		}

		#region R#

		public bool Equals(ServiceConfiguration other)
		{
			return Startup == other.Startup && Status == other.Status;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			return obj is ServiceConfiguration && Equals((ServiceConfiguration) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((int) Startup*397) ^ (int) Status;
			}
		}

		public static bool operator ==(ServiceConfiguration left, ServiceConfiguration right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ServiceConfiguration left, ServiceConfiguration right)
		{
			return !left.Equals(right);
		}

		#endregion
	}
}