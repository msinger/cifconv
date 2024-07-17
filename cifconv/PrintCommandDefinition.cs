using System.Text;

namespace cifconv
{
	public class PrintCommandDefinition : CommandDefinition
	{
		public readonly string Text;

		public PrintCommandDefinition(Position pos, string text) : base(pos)
		{
			Text = text;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("1 \"");
			sb.Append(Text.Escape());
			sb.Append("\";");
			return sb.ToString();
		}
	}
}
