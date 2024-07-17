namespace cifconv
{
	public abstract class ParserToken
	{
		public readonly Position Pos;
		public readonly int Line;
		public readonly int Col;

		protected ParserToken(Position pos)
		{
			Pos = pos;
		}
	}
}
