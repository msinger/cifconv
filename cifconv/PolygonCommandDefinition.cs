using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace cifconv
{
	public class PolygonCommandDefinition : CommandDefinition
	{
		public readonly List<Point> Points;

		public PolygonCommandDefinition(Position pos) : base(pos)
		{
			Points = new List<Point>();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("P");
			foreach (var p in Points)
			{
				sb.Append(" ");
				sb.Append(p.X.ToString(CultureInfo.InvariantCulture));
				sb.Append(" ");
				sb.Append(p.Y.ToString(CultureInfo.InvariantCulture));
			}
			sb.Append(";");
			return sb.ToString();
		}
	}
}
