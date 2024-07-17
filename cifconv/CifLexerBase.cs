using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace cifconv
{
	public abstract partial class CifLexerBase
	{
		private LexerState state = LexerState.End;
		private uint commentDepth = 0;
		private Position commentPos;
		private Position lastCommandPos;
		private bool garbageAtEnd = false;

		protected CifLexerBase() { }

		public virtual void NextFile(string file)
		{
			if (commentDepth > 0)
				Console.Error.WriteLine("Warning: " + commentPos + ": Comment extends past end of file.");
			commentDepth = 0;
			if (state != LexerState.End)
			{
				Console.Error.WriteLine("Warning: " + lastCommandPos + ": File not terminated by End command.");
				if (state != LexerState.Command)
					throw new CifFormatException(lastCommandPos, "Command not terminated by semicolon.");
			}
			state = LexerState.Command;
			garbageAtEnd = false;
			lastCommandPos = new Position(file, 0, 1, 1);
		}

		protected LinkedListNode<LexerToken> Lex(string line)
		{
			int pos = 0;
			return Lex(line, null, 1, ref pos);
		}

		protected LinkedListNode<LexerToken> Lex(string line, string file, int lineNum, ref int pos)
		{
			LinkedList<LexerToken> t = new LinkedList<LexerToken>();
			StringBuilder sb = new StringBuilder();
			int col = -1;
			char c = '\0';
			Position fpos = new Position();
			bool startInsideComment = false;

			if (commentDepth > 0)
			{
				col++;
				if (line.Length == col)
					return t.First;
				startInsideComment = true;
				goto comment;
			}

			pos--;
		read:
			pos++;
			col++;
		next:
			fpos = new Position(file, pos, lineNum, col + 1);
			if (line.Length == col)
				return t.First;

			c = line[col];

		comment:
			// Comment?
			if ((c == '(' && state != LexerState.UserText) || startInsideComment) // Parentheses may be part of name after user command 9.
			{
				if (!startInsideComment)
				{
					commentDepth = 1;
					commentPos   = fpos;
				}
				while (commentDepth > 0)
				{
					if (!startInsideComment)
					{
						col++;
						if (line.Length == col)
							return t.First;
						pos++;
					}
					startInsideComment = false;
					c = line[col];
					if (c == '(')
						commentDepth++;
					if (c == ')')
						commentDepth--;
				}
				goto read;
			}

			if (c == ';')
			{
				if (state != LexerState.End)
				{
					t.AddLast(new LexerToken(fpos, LexerTokenType.EOT));
					state = LexerState.Command;
				}
				goto read;
			}

			// Space, Tab, ...?
			if (char.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator || char.IsControl(c))
				goto read;

			switch (state)
			{
			case LexerState.Command:
				if (char.IsUpper(c))
				{
					switch (c)
					{
						case 'L': state = LexerState.Shortname;  break;
						case 'D': state = LexerState.Definition; break;
						case 'C': state = LexerState.Call;       break;
						case 'E': state = LexerState.End;        break;
						default:  state = LexerState.Integers;   break;
					}
					lastCommandPos = fpos;
					if (c != 'E')
						t.AddLast(new LexerToken(fpos, LexerTokenType.Name, c.ToString()));
					goto read;
				}

				// User command?
				if (char.IsDigit(c))
				{
					sb.Clear();
					while (char.IsDigit(c))
					{
						sb.Append(c);
						col++;
						if (line.Length == col)
							break;
						pos++;
						c = line[col];
					}
					string str = sb.ToString();
					if (str == "0")
						throw new CifFormatException(fpos, "Include command (0) not implemented.");
					state = LexerState.UserText;
					lastCommandPos = fpos;
					t.AddLast(new LexerToken(fpos, LexerTokenType.Name, str));
					goto next;
				}

				break;

			case LexerState.Definition:
				if (!char.IsDigit(c) && !char.IsUpper(c) && c != '-')
					goto read;
				state = LexerState.Integers;
				t.AddLast(new LexerToken(fpos, LexerTokenType.Name, c.ToString()));
				goto read;

			case LexerState.Shortname:
				if (!char.IsDigit(c) && !char.IsUpper(c) && c != '-')
					goto read;
				sb.Clear();
				while (char.IsDigit(c) || char.IsUpper(c))
				{
					sb.Append(c);
					col++;
					if (line.Length == col)
						break;
					pos++;
					c = line[col];
				}
				state = LexerState.Integers;
				t.AddLast(new LexerToken(fpos, LexerTokenType.Name, sb.ToString()));
				goto next;

			case LexerState.Integers:
			case LexerState.Call:
				if (!char.IsDigit(c) && c != '-')
					goto read;
				{
					bool first = true;
					sb.Clear();
					while (char.IsDigit(c) || (first && c == '-'))
					{
						first = false;
						sb.Append(c);
						col++;
						if (line.Length == col)
							break;
						pos++;
						c = line[col];
					}
					string str = sb.ToString();
					long val;
					if (!long.TryParse(str, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out val))
						throw new CifFormatException(fpos, "Invalid integer number encountered by lexer.");
					t.AddLast(new LexerToken(fpos, LexerTokenType.Value, val));
					if (state == LexerState.Call)
						state = LexerState.Transformation;
					goto next;
				}

			case LexerState.Transformation:
				if (!char.IsDigit(c) && !char.IsUpper(c) && c != '-')
					goto read;
				if (char.IsUpper(c))
				{
					t.AddLast(new LexerToken(fpos, LexerTokenType.Name, c.ToString()));
					goto read;
				}
				else
				{
					bool first = true;
					sb.Clear();
					while (char.IsDigit(c) || (first && c == '-'))
					{
						first = false;
						sb.Append(c);
						col++;
						if (line.Length == col)
							break;
						pos++;
						c = line[col];
					}
					string str = sb.ToString();
					long val;
					if (!long.TryParse(str, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out val))
						throw new CifFormatException(fpos, "Invalid integer number encountered by lexer.");
					t.AddLast(new LexerToken(fpos, LexerTokenType.Value, val));
					if (state == LexerState.Call)
						state = LexerState.Transformation;
					goto next;
				}

			case LexerState.UserText:
				// String?
				if (c == '"')
				{
					bool escaped = false;
					sb.Clear();
					while (true)
					{
						col++;
						if (line.Length == col)
							throw new CifFormatException(fpos, "Unterminated string literal encountered by lexer.");
						pos++;
						c = line[col];
						if (escaped)
						{
							switch (c)
							{
								case 'n': sb.Append('\n'); break;
								case 't': sb.Append('\t'); break;
								default:  sb.Append(c);    break;
							}
							escaped = false;
							continue;
						}
						else if (c != '"')
						{
							switch (c)
							{
								case '\\': escaped = true; break;
								default:   sb.Append(c);   break;
							}
							continue;
						}
						break;
					}
					t.AddLast(new LexerToken(fpos, LexerTokenType.String, sb.ToString()));
					goto read;
				}

				// Start of name or value?
				{
					bool is_name = false;
					bool first = true;
					sb.Clear();
					while (char.GetUnicodeCategory(c) != UnicodeCategory.SpaceSeparator &&
					       !char.IsControl(c) &&
					       c != ';' &&
					       c != '"')
					{
						sb.Append(c);
						if (!char.IsDigit(c) && c != '-')
							is_name = true;
						if (!first && c == '-')
							is_name = true;
						first = false;
						col++;
						if (line.Length == col)
							break;
						pos++;
						c = line[col];
					}
					string str = sb.ToString();
					if (is_name || str == "-")
					{
						t.AddLast(new LexerToken(fpos, LexerTokenType.Name, str));
						goto next;
					}
					long val;
					if (!long.TryParse(str, NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo, out val))
						throw new CifFormatException(fpos, "Invalid integer number encountered by lexer.");
					t.AddLast(new LexerToken(fpos, LexerTokenType.Value, val));
					goto next;
				}

			case LexerState.End:
				if (!char.IsDigit(c) && !char.IsUpper(c) && c != '-')
					goto read;
				if (garbageAtEnd)
					goto read;
				garbageAtEnd = true;
				Console.Error.WriteLine("Warning: " + fpos + ": Ignoring non-blank characters after End command.");
				goto read;
			}

			throw new CifFormatException(fpos, "Unknown input character encountered by lexer.");
		}
	}
}
