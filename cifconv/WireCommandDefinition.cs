using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace cifconv
{
	public class WireCommandDefinition : CommandDefinition
	{
		public readonly long Width;
		public readonly List<Point> Points;

		public WireCommandDefinition(Position pos, long width) : base(pos)
		{
			Width  = width;
			Points = new List<Point>();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("W ");
			sb.Append(Width.ToString(CultureInfo.InvariantCulture));
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
