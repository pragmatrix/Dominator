using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Dominator.Windows10.Settings;
using NUnit.Framework;

namespace Dominator.Tests
{
	[TestFixture]
    public class ServiceTests
    {
		[Test]
		public void Existing()
		{
			bool shouldExist = ServiceTools.IsExisting("cdfs");
			Assert.True(shouldExist);
		}

		[Test]
		public void NotExisting()
		{
			bool shouldNotExist = ServiceTools.IsExisting("no_known_service_has_this_name");
			Assert.False(shouldNotExist);
		}

		[Test]
		public void Startup()
		{
			var startup = ServiceTools.TryGetServiceStartup("DnsCache");
			Assert.That(startup, Is.EqualTo(ServiceStartup.Automatic));
		}

		[Test]
		public void StopAndStart()
		{
			// Diagnostic Policy Service
			Assert.That(ServiceTools.IsExisting("DPS"));
			Assert.That(ServiceTools.Status("DPS"), Is.EqualTo(ServiceControllerStatus.Running));
            ServiceTools.Stop("DPS", TimeSpan.FromMilliseconds(5000));
			ServiceTools.Start("DPS", TimeSpan.FromMilliseconds(5000));
		}

		[Test, Ignore]
		public void StartDPS()
		{
			ServiceTools.Start("DPS", TimeSpan.FromMilliseconds(5000));
		}

	}
}
