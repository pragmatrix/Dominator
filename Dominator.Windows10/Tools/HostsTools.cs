using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dominator.Windows10.Tools
{
	static class HostsTools
	{
		public static readonly string SystemHostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

		public static string[] ReadSystemHostsFile()
		{
			return ReadHostsFile(SystemHostsFilePath);
		}

		public static void WriteSystemHostsFile(IEnumerable<string> lines)
		{
			WriteHostsFile(SystemHostsFilePath, lines);
		}

		public static string[] ReadHostsFile(string path)
		{
			return File.ReadAllLines(path, Encoding.ASCII);
		}

		public static void WriteHostsFile(string path, IEnumerable<string> lines)
		{
			File.WriteAllLines(path, lines, Encoding.ASCII);
		}

		public static HostLine[] SafeParseHostLines(this IEnumerable<string> lines)
		{
			return lines.Select(HostLine.SafeParse).ToArray();
		}

		public static HostLine[] Merge(this IEnumerable<HostLine> left, IEnumerable<HostEntry> right)
		{
			var todo = right.ToDictionary(he => he.Host, he => he);

			// first replace the ones that are already existing and delete them from the dictionary.

			var replaced = left.Select(line =>
			{
				if (line.Kind != HostLineKind.HostEntry || !todo.ContainsKey(line.Entry_.Value.Host))
					return line;

				var host = line.Entry_.Value.Host;
				var entry = todo[host];
				todo.Remove(host);
				return new HostLine(HostLineKind.HostEntry, entry, comment_: line.Comment_);
			}).ToArray();

			// then add the remaining ones.

			var remaining = todo.Values.Select(e => HostLine.FromEntry(e));
			return replaced.Concat(remaining).ToArray();
		}

		public static HostLine[] FilterHosts(this IEnumerable<HostLine> left, IEnumerable<string> hosts)
		{
			var table = new HashSet<string>(hosts);

			return left.Where(l => l.Kind != HostLineKind.HostEntry || !table.Contains(l.Entry_.Value.Host)).ToArray();
		}

		public static bool ContainsAllHostEntries(this IEnumerable<HostLine> lines, IEnumerable<HostEntry> entries)
		{
			var tocheck = entries.ToDictionary(l => l.Host, l => l);
			foreach (var line in lines)
			{
				if (line.Kind != HostLineKind.HostEntry)
					continue;

				var entry = line.Entry_.Value;
				var host = entry.Host;
				if (!tocheck.ContainsKey(host))
					continue;

				var entryToCheck = tocheck[line.Entry_.Value.Host];

				if (line.Entry_.Value.IP != entryToCheck.IP)
					continue;

				tocheck.Remove(host);
			}

			return tocheck.Count == 0;
		}

		public static HostEntry[] ExtractEntries(this IEnumerable<HostLine> lines)
		{
			return lines
				.Where(l => l.Kind == HostLineKind.HostEntry)
				.Select(l => l.Entry_.Value)
				.ToArray();
		}

		public static string[] ToLines(this IEnumerable<HostLine> lines)
		{
			return lines.Select(l => l.Line).ToArray();
		}
	}
}
