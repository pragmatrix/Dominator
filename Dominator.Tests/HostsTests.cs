using System.IO;
using System.Linq;
using System.Text;
using Dominator.Windows10.Tools;
using NUnit.Framework;

namespace Dominator.Tests
{
	[TestFixture]
	public class HostsTests
	{
		[Test]
		public void readHostsFile()
		{
			var hosts = HostsTools.ReadHostsFile("hosts.txt");
			Assert.That(hosts.Length, Is.EqualTo(90));
		}

		// note NCrunch seems to add BOMs to the text files :(, so we
		// ignore this test for now and run it manually with R#.
		[Test, Ignore]
		public void readingAndWritingHostsFileDoesNotChangeIt()
		{
			var binaryBefore = File.ReadAllBytes("hosts.txt");

			var lines = HostsTools.ReadHostsFile("hosts.txt");
			HostsTools.WriteHostsFile("hosts.txt.bak", lines);

			var binaryAfter = File.ReadAllBytes("hosts.txt.bak");

			CollectionAssert.AreEqual(binaryBefore, binaryAfter);
		}

		[Test, Ignore]
		public void parsingAndSerializingTheHostsFileDoesNotChangeIt()
		{
			var binaryBefore = File.ReadAllBytes("hosts.txt");

			var lines = HostsTools.ReadHostsFile("hosts.txt").SafeParseHostLines();
			HostsTools.WriteHostsFile("hosts.txt.bak", lines.ToLines());

			var binaryAfter = File.ReadAllBytes("hosts.txt.bak");

			CollectionAssert.AreEqual(binaryBefore, binaryAfter);
		}

		[Test]
		public void emptyStringIsEmptyLine()
		{
			var l = HostLine.Parse("");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.EmptyLine));
		}

		[Test]
		public void spaceIsEmptyLineToo()
		{
			var l = HostLine.Parse(" ");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.EmptyLine));
		}

		[Test]
		public void emptyCommentLine()
		{
			var l = HostLine.Parse("#");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.CommentLine));
			Assert.That(l.Comment_, Is.EqualTo("#"));
		}

		[Test]
		public void commentLine()
		{
			var l = HostLine.Parse("#hello");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.CommentLine));
			Assert.That(l.Comment_, Is.EqualTo("#hello"));
		}

		[Test]
		public void commentLinePreservesWhitespaceBeforeComment()
		{
			var l = HostLine.Parse("\t #hello");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.CommentLine));
			Assert.That(l.Comment_, Is.EqualTo("\t #hello"));
		}

		[Test]
		public void commentAfterIP()
		{
			var l = HostLine.Parse("0#hello");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.ParseError));
			// that could be a better error like found no URL, or early comment or what.
			Assert.That(l.Error_, Is.EqualTo(HostLineError.NoSpaceOrTabDelimiterFound));
		}

		[Test]
		public void ipAndHost()
		{
			var l = HostLine.Parse("0 1");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.HostEntry));
			Assert.That(l.Entry_.Value.IP, Is.EqualTo("0"));
			Assert.That(l.Entry_.Value.Host, Is.EqualTo("1"));
		}

		[Test]
		public void ipAndHostWithComment()
		{
			var l = HostLine.Parse("0 1#hello");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.HostEntry));
			Assert.That(l.Entry_.Value.IP, Is.EqualTo("0"));
			Assert.That(l.Entry_.Value.Host, Is.EqualTo("1"));
			Assert.That(l.Comment_, Is.EqualTo("#hello"));
			Assert.That(l.Line, Is.EqualTo("0 1#hello"));
		}

		[Test]
		public void ipAndHostWithMoreWhitespaceInBetween()
		{
			var l = HostLine.Parse("\t 0\t 1\t");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.HostEntry));
			Assert.That(l.Entry_.Value.IP, Is.EqualTo("0"));
			Assert.That(l.Entry_.Value.Host, Is.EqualTo("1"));
			Assert.That(l.Line, Is.EqualTo("\t 0\t 1\t"));
		}

		[Test]
		public void ipAndHostAndWhitespaceBeforeComment()
		{
			var l = HostLine.Parse("0 1 #hello");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.HostEntry));
			Assert.That(l.Entry_.Value.IP, Is.EqualTo("0"));
			Assert.That(l.Entry_.Value.Host, Is.EqualTo("1"));
			Assert.That(l.Comment_, Is.EqualTo(" #hello"));
		}

		[Test]
		public void excessCharacters()
		{
			var line = "0 1 f";
			var l = HostLine.Parse(line);
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.ParseError));
			Assert.That(l.Error_.Value, Is.EqualTo(HostLineError.FoundExcessCharactersAfterIPAndHost));
			Assert.That(l.Line, Is.EqualTo(line));
		}

		[Test]
		public void parseTestingHostFile()
		{
			var lines = File.ReadAllLines("hosts.txt", Encoding.ASCII);
			Assert.That(lines.Length, Is.EqualTo(90));
			var allComments = lines.Take(17)
				.Select(HostLine.Parse)
				.Select(hl => hl.Kind)
				.All(x => x == HostLineKind.CommentLine);
			Assert.True(allComments);

			var emptyLine =
				lines.Skip(17).Take(1).Select(HostLine.Parse).Select(hl => hl.Kind).All(x => x == HostLineKind.EmptyLine);
			Assert.True(emptyLine);

			// 3 comment lines skippt

			var successfullyParsed = 
				lines.Skip(18+3).Take(57).Select(HostLine.Parse).Select(hl => hl.Kind).All(x => x == HostLineKind.HostEntry);
			Assert.True(successfullyParsed);

			// further empty and some more other lines skipped
		}

		[Test]
		public void hostsContainsAllWatsonEntries()
		{
			var lines = HostsTools.ReadHostsFile("hosts.txt").SafeParseHostLines();
			var watson = HostsTools.ReadHostsFile("watson.txt").SafeParseHostLines().ExtractEntries();
			Assert.That(watson.Length, Is.EqualTo(2));

			var containsWatson = lines.ContainsAllHostEntries(watson);
			Assert.True(containsWatson);
		}

		[Test]
		public void merge()
		{
			var lines = HostsTools.ReadHostsFile("hosts.txt").SafeParseHostLines();
			var skypeAds = HostsTools.ReadHostsFile("skype-ads.txt").SafeParseHostLines().ExtractEntries();
			Assert.That(skypeAds.Length, Is.EqualTo(9));

			var containsSkypeAds = lines.ContainsAllHostEntries(skypeAds);
			Assert.False(containsSkypeAds);

			// then we merge them (in this case, all entries are replaced, because they have just another IP, none is added).
			var merged = lines.Merge(skypeAds);
			Assert.That(merged.Length, Is.EqualTo(lines.Length));

			var containsSkypeAds2 = merged.ContainsAllHostEntries(skypeAds);
			Assert.True(containsSkypeAds2);

			// no merge and add one
			var newAds = skypeAds.Concat(new[] {new HostEntry("ip", "host")});
			var merged2 = lines.Merge(newAds);
			Assert.True(merged2.ContainsAllHostEntries(newAds));
			Assert.That(merged2.Length, Is.EqualTo(merged.Length+1));
		}

		[Test]
		public void filter()
		{
			var lines = HostsTools.ReadHostsFile("hosts.txt").SafeParseHostLines();
			var filtered = lines.FilterHosts(new[] {"watson.live.com", "watson.microsoft.com"});
			Assert.That(filtered.Length, Is.EqualTo(lines.Length-2));
		}

		[Test]
		public void emptyFilterFiltersNothing()
		{
			var lines = HostsTools.ReadHostsFile("hosts.txt").SafeParseHostLines();
			var filtered = lines.FilterHosts(new string[0]);
			Assert.That(filtered.Length, Is.EqualTo(lines.Length));
		}

		[Test]
		public void duplicatesAreFiltered()
		{
			var lines = HostsTools.ReadHostsFile("hosts.txt").SafeParseHostLines()
				.Concat(HostsTools.ReadHostsFile("hosts.txt").SafeParseHostLines())
				.ToArray();
			var lines2 = lines.FilterHosts(new[] { "watson.live.com", "watson.microsoft.com" });
			Assert.That(lines2.Length, Is.EqualTo(lines.Length-4));
		}

		[Test]
		public void hostLineSerializesProperly()
		{
			var hostLine = new HostLine(HostLineKind.HostEntry, new HostEntry("0.1.2.3", "hostname"), comment_: " # comment");
			Assert.That(hostLine.Line, Is.EqualTo("0.1.2.3 hostname # comment"));
		}
	}
}
