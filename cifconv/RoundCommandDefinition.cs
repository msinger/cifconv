using System.Globalization;
using System.Text;

namespace cifconv
{
	public class RoundCommandDefinition : CommandDefinition
	{
		public readonly long Diameter;
		public readonly Point Center;

		public RoundCommandDefinition(Position pos, long diameter, Point center) : base(pos)
		{
			Diameter = diameter;
			Center   = center;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("R ");
			sb.Append(Diameter.ToString(CultureInfo.InvariantCulture));
			sb.Append(" ");
			sb.Append(Center.X.ToString(CultureInfo.InvariantCulture));
			sb.Append(" ");
			sb.Append(Center.Y.ToString(CultureInfo.InvariantCulture));
			sb.Append(";");
			return sb.ToString();
		}
	}
}
