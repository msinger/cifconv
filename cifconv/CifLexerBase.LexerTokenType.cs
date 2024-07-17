namespace cifconv
{
	public abstract partial class CifLexerBase
	{
		[System.Serializable]
		protected enum LexerTokenType
		{
			EOT,    // End of Text
			Name,   // B, L, CPG, ...
			String, // String literals "..."
			Value,  // 105, -5834, ...
		}
	}
}
