namespace cifconv
{
	public class MirrorDefinition : TransformationDefinition
	{
		public readonly MirrorAxis Axis;

		public MirrorDefinition(Position pos, MirrorAxis axis) : base(pos)
		{
			Axis = axis;
		}

		public override string ToString()
		{
			switch (Axis)
			{
				case MirrorAxis.X: return "M X";
				case MirrorAxis.Y: return "M Y";
				default:           return "";
			}
		}
	}
}
