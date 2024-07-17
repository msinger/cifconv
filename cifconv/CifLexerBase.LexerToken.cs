using System.Globalization;

namespace cifconv
{
	public abstract partial class CifLexerBase
	{
		protected class LexerToken
		{
			public readonly Position       Pos;
			public readonly LexerTokenType Type;
			public readonly string         String = null;
			public readonly long           Value  = 0;

			public LexerToken(Position pos, LexerTokenType type)
			{
				Pos  = pos;
				Type = type;
			}

			public LexerToken(Position pos, LexerTokenType type, string str)
				: this(pos, type)
			{
				String = str;
			}

			public LexerToken(Position pos, LexerTokenType type, long val)
				: this(pos, type)
			{
				Value = val;
			}

			public override string ToString()
			{
				string s = null;
				switch (Type)
				{
					case LexerTokenType.EOT:    s = "<EOT>";                                        break;
					case LexerTokenType.Name:   s = "'" + String + "'";                             break;
					case LexerTokenType.String: s = "\"" + String + "\"";                           break;
					case LexerTokenType.Value:  s = Value.ToString(NumberFormatInfo.InvariantInfo); break;
				}
				return s;
			}
		}
	}
}
