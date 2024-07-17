using System.Globalization;
using System.Text;

namespace cifconv
{
	public class DefStartCommandDefinition : DefCommandDefinition
	{
		public readonly uint Symbol;
		public readonly long A, B;

		public DefStartCommandDefinition(Position pos, uint symbol, long a, long b) : base(pos)
		{
			Symbol = symbol;
			A      = a;
			B      = b;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("DS ");
			sb.Append(Symbol.ToString(CultureInfo.InvariantCulture));
			sb.Append(" ");
			sb.Append(A.ToString(CultureInfo.InvariantCulture));
			sb.Append(" ");
			sb.Append(B.ToString(CultureInfo.InvariantCulture));
			sb.Append(";");
			return sb.ToString();
		}
	}
}
