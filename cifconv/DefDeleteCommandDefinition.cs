using System.Globalization;
using System.Text;

namespace cifconv
{
	public class DefDeleteCommandDefinition : DefCommandDefinition
	{
		public readonly uint MinSymbol;

		public DefDeleteCommandDefinition(Position pos, uint minSymbol) : base(pos)
		{
			MinSymbol = minSymbol;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("DD ");
			sb.Append(MinSymbol.ToString(CultureInfo.InvariantCulture));
			sb.Append(";");
			return sb.ToString();
		}
	}
}
