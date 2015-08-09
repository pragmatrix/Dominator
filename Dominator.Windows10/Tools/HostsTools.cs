using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dominator.Windows10.Tools
{
	class HostsTools
	{
		public static readonly string HostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

		public static string[] ReadHostsFile()
		{
			return File.ReadAllLines(HostsFilePath, Encoding.ASCII);
		}

		public static void WriteHostsFile(IEnumerable<string> lines)
		{
			File.WriteAllLines(HostsFilePath, lines, Encoding.ASCII);
		}

		public static HostLine[] ParseHostLines(IEnumerable<string> lines)
		{
			return lines.Select(HostLine.SafeParse).ToArray();
		}

		static void mergeHosts(HostLine[] left, HostEntry[] right)
		{
			


		}
	}
}
