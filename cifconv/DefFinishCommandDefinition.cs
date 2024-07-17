namespace cifconv
{
	public class DefFinishCommandDefinition : DefCommandDefinition
	{
		public DefFinishCommandDefinition(Position pos) : base(pos)
		{
		}

		public override string ToString()
		{
			return "DF;";
		}
	}
}
