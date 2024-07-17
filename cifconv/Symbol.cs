using System.Collections.Generic;

namespace cifconv
{
	public class Symbol
	{
		public readonly uint Index;
		public readonly long A, B;
		public readonly List<CommandDefinition> Commands;
		public string Name;

		public Symbol(uint index, long a, long b)
		{
			Index    = index;
			A        = a;
			B        = b;
			Commands = new List<CommandDefinition>();
			Name     = null;
		}
	}
}
