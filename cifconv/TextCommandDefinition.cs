using System.Collections.Generic;
using System.Text;

namespace cifconv
{
	public class TextCommandDefinition : CommandDefinition
	{
		public readonly string Text;
		public readonly List<TransformationDefinition> Transformations;

		public TextCommandDefinition(Position pos, string text) : base(pos)
		{
			Text            = text;
			Transformations = new List<TransformationDefinition>();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("2 \"");
			sb.Append(Text.Escape());
			sb.Append("\"");
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
