using System.Text;

namespace cifconv
{
	public class LayerCommandDefinition : CommandDefinition
	{
		public readonly string Layer;

		public LayerCommandDefinition(Position pos, string layer) : base(pos)
		{
			Layer = layer;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("L ");
			sb.Append(Layer);
			sb.Append(";");
			return sb.ToString();
		}
	}
}
