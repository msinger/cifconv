using System;

namespace cifconv
{
	[Serializable]
	public struct Position : System.Runtime.Serialization.ISerializable
	{
		public string File;
		public int    Pos;
		public int    Line;
		public int    Col;

		public Position(string file, int pos, int line, int col)
		{
			File = file;
			Pos  = pos;
			Line = line;
			Col  = col;
		}

		private Position(System.Runtime.Serialization.SerializationInfo info,
		                 System.Runtime.Serialization.StreamingContext  context)
		{
			File = info.GetString("Filename");
			Pos  = info.GetInt32("Position");
			Line = info.GetInt32("Line");
			Col  = info.GetInt32("Column");
		}

		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info,
		                          System.Runtime.Serialization.StreamingContext  context)
		{
			info.AddValue("Filename", File, typeof(string));
			info.AddValue("Position", Pos,  typeof(int));
			info.AddValue("Line",     Line, typeof(int));
			info.AddValue("Column",   Col,  typeof(int));
		}

		public override string ToString()
		{
			string s = "";
			if (Line != 0 && Col != 0)
				s = Line.ToString() + ":" + Col.ToString();
			else if (Col != 0)
				s = Col.ToString();
			if (!string.IsNullOrEmpty(File))
				s = File + ":" + s;
			return s;
		}
	}
}
