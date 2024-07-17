using System.Text;

namespace cifconv
{
	public class SymbolNameCommandDefinition : CommandDefinition
	{
		public readonly string Text;

		public SymbolNameCommandDefinition(Position pos, string text) : base(pos)
		{
			Text = text;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("9 ");
			sb.Append(Text);
			sb.Append(";");
			return sb.ToString();
		}
	}
}
