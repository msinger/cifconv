using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace cifconv
{
	public class CallCommandDefinition : CommandDefinition
	{
		public readonly uint Symbol;
		public readonly List<TransformationDefinition> Transformations;

		public CallCommandDefinition(Position pos, uint symbol) : base(pos)
		{
			Symbol          = symbol;
			Transformations = new List<TransformationDefinition>();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("C ");
			sb.Append(Symbol.ToString(CultureInfo.InvariantCulture));
			foreach (var t in Transformations)
			{
				sb.Append(" ");
				sb.Append(t.ToString());
			}
			sb.Append(";");
			return sb.ToString();
		}
	}
}
