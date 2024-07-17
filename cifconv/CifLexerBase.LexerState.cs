namespace cifconv
{
	public abstract partial class CifLexerBase
	{
		[System.Serializable]
		protected enum LexerState
		{
			Command = 0,     // Expecting command
			Definition,      // Expecting definition sub command S, F or D
			Shortname,       // Expecting layer shortname
			Integers,        // Expecting integers
			Call,            // Expecting single integer after Call command
			Transformation,  // Expecting transformation list for Call command
			UserText,        // Expecting any arguments for user extension command
			End,             // Expecting only blanks after End command
		}
	}
}
