namespace Dominator.Windows10.Tools
{
	struct HostEntry
	{
		public HostEntry(string ip, string host)
		{
			IP = ip;
			Host = host;
		}

		public readonly string IP;
		public readonly string Host;

		#region R#

		public bool Equals(HostEntry other)
		{
			return string.Equals(IP, other.IP) && string.Equals(Host, other.Host);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			return obj is HostEntry && Equals((HostEntry)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((IP != null ? IP.GetHashCode() : 0) * 397) ^ (Host != null ? Host.GetHashCode() : 0);
			}
		}

		public static bool operator ==(HostEntry left, HostEntry right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(HostEntry left, HostEntry right)
		{
			return !left.Equals(right);
		}

		#endregion
	}
}