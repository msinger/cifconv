using System.Globalization;
using System.Text;

namespace cifconv
{
	public class RotationDefinition : TransformationDefinition
	{
		public readonly Point Direction;

		public RotationDefinition(Position pos, Point direction) : base(pos)
		{
			Direction = direction;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("R ");
			sb.Append(Direction.X.ToString(CultureInfo.InvariantCulture));
			sb.Append(" ");
			sb.Append(Direction.Y.ToString(CultureInfo.InvariantCulture));
			sb.Append(";");
			return sb.ToString();
		}
	}
}
