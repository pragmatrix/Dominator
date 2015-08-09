using System;

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
		public readonly string Comment_;
		public readonly HostLineError? Error_;

		public HostLine(HostLineKind kind, HostEntry? entry_ = null, string comment_ = null, HostLineError? error_ = null)
		{
			Kind = kind;
			Entry_ = entry_;
			Comment_ = comment_;
			Error_ = error_;
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
			line = line.Trim();
			if (line == "")
				return new HostLine(HostLineKind.EmptyLine);

			if (line.StartsWith("#"))
				return new HostLine(HostLineKind.CommentLine, comment_: line.Substring(1));

			var firstSpaceOrTab = line.IndexOfAny(new[] {' ', '\t'});
			if (firstSpaceOrTab == -1)
				return Error(HostLineError.NoSpaceOrTabDelimiterFound);

			var ip = line.Substring(0, firstSpaceOrTab);
			line = line.Substring(firstSpaceOrTab).TrimStart();
			var urlEnd = line.IndexOfAny(new[] {' ', '\t', '#'});
			if (urlEnd == -1)
			{
				if (line.Length == 0)
					return Error(HostLineError.ZeroLengthURL);
				return new HostLine(HostLineKind.HostEntry, new HostEntry(ip, line));
			}
			var host = line.Substring(0, urlEnd);
			var rest = line.Substring(urlEnd).TrimStart();
			if (rest.StartsWith("#"))
				return new HostLine(HostLineKind.HostEntry, new HostEntry(ip, host), rest.Substring(1));
			return Error(HostLineError.FoundExcessCharactersAfterIPAndHost);
		}

		static HostLine Error(HostLineError text)
		{
			return new HostLine(HostLineKind.ParseError, error_: text);
		}
	}
}