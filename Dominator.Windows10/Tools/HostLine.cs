using System;
using System.Text;

namespace Dominator.Windows10.Tools
{
	enum HostLineKind
	{
		CommentLine,
		EmptyLine,
		HostEntry,
		ParseError
	}

	enum HostLineError
	{
		NoSpaceOrTabDelimiterFound,
		ZeroLengthURL,
		FoundExcessCharactersAfterIPAndHost,
		InternalError
	}

	struct HostLine
	{
		public readonly HostLineKind Kind;
		public readonly HostEntry? Entry_;
		// note that comments do include all the prefixing whitespace and '#'
		public readonly string Comment_;
		public readonly HostLineError? Error_;
		public readonly string Line;

		public HostLine(HostLineKind kind, HostEntry? entry_ = null, string comment_ = null, HostLineError? error_ = null, string line_ = null)
		{
			Kind = kind;
			Entry_ = entry_;
			Comment_ = comment_;
			Error_ = error_;
			Line = line_ ?? mkLine(Entry_, Comment_);
		}

		static string mkLine(HostEntry? entry_, string comment_)
		{
			var sb = new StringBuilder();
			if (entry_ != null)
			{
				sb.Append(entry_.Value.Host);
				sb.Append(' ');
				sb.Append(entry_.Value.IP);
			}

			if (comment_ != null)
				sb.Append(comment_);

			return sb.ToString();
		}

		public override string ToString()
		{
			return Line;
		}

		public static HostLine FromEntry(HostEntry entry, string comment_ = null)
		{
			return new HostLine(HostLineKind.HostEntry, entry_: entry, comment_: comment_);
		}

		public static HostLine SafeParse(string line)
		{
			// The line parser should be pretty robust, but we'll never know for sure.
			try
			{
				return Parse(line);
			}
			catch (Exception)
			{
				return new HostLine(HostLineKind.ParseError, error_: HostLineError.InternalError);
			}
		}

		public static HostLine Parse(string line)
		{
			var todo = line.Trim();
			if (todo == "")
				return new HostLine(HostLineKind.EmptyLine);

			if (todo.StartsWith("#"))
				return new HostLine(HostLineKind.CommentLine, comment_: line, line_: line);

			var firstSpaceOrTab = todo.IndexOfAny(new[] {' ', '\t'});
			if (firstSpaceOrTab == -1)
				return Error(HostLineError.NoSpaceOrTabDelimiterFound, line);

			var ip = todo.Substring(0, firstSpaceOrTab);
			todo = todo.Substring(firstSpaceOrTab).TrimStart();
			var urlEnd = todo.IndexOfAny(new[] {' ', '\t', '#'});
			if (urlEnd == -1)
			{
				if (todo.Length == 0)
					return Error(HostLineError.ZeroLengthURL, line);
				return new HostLine(HostLineKind.HostEntry, new HostEntry(ip, todo), line_: line);
			}
			var host = todo.Substring(0, urlEnd);
			var rest = todo.Substring(urlEnd);
			todo = rest.TrimStart();
			if (todo.StartsWith("#"))
				return new HostLine(HostLineKind.HostEntry, new HostEntry(ip, host), rest, line_: line);
			return Error(HostLineError.FoundExcessCharactersAfterIPAndHost, line);
		}

		static HostLine Error(HostLineError error, string line)
		{
			return new HostLine(HostLineKind.ParseError, error_: error, line_: line);
		}
	}
}