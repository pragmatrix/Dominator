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
		[Test, Ignore]
		public void readHostsFile()
		{
			var hosts = HostsTools.ReadHostsFile();
			Assert.That(hosts.Length, Is.GreaterThan(0));
		}

		[Test, Ignore]
		public void readingAndWritingHostsFileDoesNotChangeIt()
		{
			var binaryBefore = File.ReadAllBytes(HostsTools.HostsFilePath);

			var lines = HostsTools.ReadHostsFile();
			HostsTools.WriteHostsFile(lines);

			var binaryAfter = File.ReadAllBytes(HostsTools.HostsFilePath);

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
			Assert.That(l.Comment_, Is.EqualTo(""));
		}

		[Test]
		public void commentLine()
		{
			var l = HostLine.Parse("#hello");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.CommentLine));
			Assert.That(l.Comment_, Is.EqualTo("hello"));
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
			// that could be a better error like found no URL, or early comment or what.
			Assert.That(l.Entry_.Value.IP, Is.EqualTo("0"));
			Assert.That(l.Entry_.Value.Host, Is.EqualTo("1"));
		}

		[Test]
		public void ipAndHostWithComment()
		{
			var l = HostLine.Parse("0 1#hello");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.HostEntry));
			// that could be a better error like found no URL, or early comment or what.
			Assert.That(l.Entry_.Value.IP, Is.EqualTo("0"));
			Assert.That(l.Entry_.Value.Host, Is.EqualTo("1"));
			Assert.That(l.Comment_, Is.EqualTo("hello"));
		}

		[Test]
		public void ipAndHostWithMoreWhitespaceInBetween()
		{
			var l = HostLine.Parse("\t 0\t 1\t");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.HostEntry));
			// that could be a better error like found no URL, or early comment or what.
			Assert.That(l.Entry_.Value.IP, Is.EqualTo("0"));
			Assert.That(l.Entry_.Value.Host, Is.EqualTo("1"));
		}

		[Test]
		public void ipAndHostAndWhitespaceBeforeComment()
		{
			var l = HostLine.Parse("0 1 #hello");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.HostEntry));
			// that could be a better error like found no URL, or early comment or what.
			Assert.That(l.Entry_.Value.IP, Is.EqualTo("0"));
			Assert.That(l.Entry_.Value.Host, Is.EqualTo("1"));
			Assert.That(l.Comment_, Is.EqualTo("hello"));
		}

		[Test]
		public void excessCharacters()
		{
			var l = HostLine.Parse("0 1 f");
			Assert.That(l.Kind, Is.EqualTo(HostLineKind.ParseError));
			// that could be a better error like found no URL, or early comment or what.
			Assert.That(l.Error_.Value, Is.EqualTo(HostLineError.FoundExcessCharactersAfterIPAndHost));
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
	}
}
