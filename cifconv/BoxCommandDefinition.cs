using System.Globalization;
using System.Text;

namespace cifconv
{
	public class BoxCommandDefinition : CommandDefinition
	{
		public readonly long Length;
		public readonly long Width;
		public readonly Point Center;
		public readonly Point Direction;

		public BoxCommandDefinition(Position pos, long length, long width,
		                            Point center, Point direction) : base(pos)
		{
			Length    = length;
			Width     = width;
			Center    = center;
			Direction = direction;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("B ");
			sb.Append(Length.ToString(CultureInfo.InvariantCulture));
			sb.Append(" ");
			sb.Append(Width.ToString(CultureInfo.InvariantCulture));
			sb.Append(" ");
			sb.Append(Center.X.ToString(CultureInfo.InvariantCulture));
			sb.Append(" ");
			sb.Append(Center.Y.ToString(CultureInfo.InvariantCulture));
			if (Direction.X != 1 || Direction.Y != 0)
			{
				sb.Append(" ");
				sb.Append(Direction.X.ToString(CultureInfo.InvariantCulture));
				sb.Append(" ");
				sb.Append(Direction.Y.ToString(CultureInfo.InvariantCulture));
			}
			sb.Append(";");
			return sb.ToString();
		}
	}
}
