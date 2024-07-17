using System.Globalization;
using System.Text;

namespace cifconv
{
	public class TranslationDefinition : TransformationDefinition
	{
		public readonly Point Point;

		public TranslationDefinition(Position pos, Point point) : base(pos)
		{
			Point = point;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("T ");
			sb.Append(Point.X.ToString(CultureInfo.InvariantCulture));
			sb.Append(" ");
			sb.Append(Point.Y.ToString(CultureInfo.InvariantCulture));
			sb.Append(";");
			return sb.ToString();
		}
	}
}
