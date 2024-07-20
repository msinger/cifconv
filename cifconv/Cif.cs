using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace cifconv
{
	public partial class Cif : CifLexerBase
	{
		public readonly List<CommandDefinition> Commands;

		public Cif() : base()
		{
			Commands = new List<CommandDefinition>();
		}

		protected static void ParseEOT(LinkedListNode<LexerToken> n)
		{
			if (n.Value.Type != LexerTokenType.EOT)
				throw new CifFormatException(n.Value.Pos, "Semicolon expected.");
		}

		protected static RotationDefinition ParseRotation(ref LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "R")
				throw new CifFormatException(n.Value.Pos, "Rotation definition expected.");
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Direction X vector expected.");
			long x = n.Value.Value;
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Direction Y vector expected.");
			long y = n.Value.Value;
			n = n.Next;

			return new RotationDefinition(me.Value.Pos, new Point(x, y));
		}

		protected static MirrorDefinition ParseMirror(ref LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "M")
				throw new CifFormatException(n.Value.Pos, "Mirror definition expected.");
			n = n.Next;

			if (n.Value.Type == LexerTokenType.Name)
			{
				n = n.Next;
				switch (n.Previous.Value.String)
				{
					case "X": return new MirrorDefinition(me.Value.Pos, MirrorAxis.X);
					case "Y": return new MirrorDefinition(me.Value.Pos, MirrorAxis.Y);
				}
				n = n.Previous;
			}

			throw new CifFormatException(n.Value.Pos, "Mirror axis (X or Y) expected.");
		}

		protected static TranslationDefinition ParseTranslation(ref LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "T")
				throw new CifFormatException(n.Value.Pos, "Translation definition expected.");
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "X coordinate expected.");
			long x = n.Value.Value;
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Y coordinate expected.");
			long y = n.Value.Value;
			n = n.Next;

			return new TranslationDefinition(me.Value.Pos, new Point(x, y));
		}

		protected static List<TransformationDefinition> ParseTransformations(ref LinkedListNode<LexerToken> n)
		{
			List<TransformationDefinition> l = new List<TransformationDefinition>();

			while (n.Value.Type == LexerTokenType.Name)
			{
				switch (n.Value.String)
				{
				case "R":
					l.Add(ParseRotation(ref n));
					break;
				case "M":
					l.Add(ParseMirror(ref n));
					break;
				case "T":
					l.Add(ParseTranslation(ref n));
					break;
				}
			}

			return l;
		}

		protected static ParserToken ParsePolygonCommand(LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "P")
				throw new CifFormatException(n.Value.Pos, "Polygon command expected.");
			n = n.Next;

			PolygonCommandDefinition p = new PolygonCommandDefinition(me.Value.Pos);

			while (n.Value.Type == LexerTokenType.Value)
			{
				long x = n.Value.Value;
				n = n.Next;

				if (n.Value.Type != LexerTokenType.Value)
					throw new CifFormatException(n.Value.Pos, "Y coordinate expected.");
				long y = n.Value.Value;
				n = n.Next;

				p.Points.Add(new Point(x, y));
			}

			ParseEOT(n);

			return p;
		}

		protected static ParserToken ParseBoxCommand(LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "B")
				throw new CifFormatException(n.Value.Pos, "Box command expected.");
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Length expected.");
			long l = n.Value.Value;
			if (l < 0)
				throw new CifFormatException(n.Value.Pos, "Positive length expected.");
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Width expected.");
			long w = n.Value.Value;
			if (w < 0)
				throw new CifFormatException(n.Value.Pos, "Positive width expected.");
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Center X coordinate expected.");
			long cx = n.Value.Value;
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Center Y coordinate expected.");
			long cy = n.Value.Value;
			n = n.Next;

			long dx = 1;
			long dy = 0;
			if (n.Value.Type == LexerTokenType.Value)
			{
				dx = n.Value.Value;
				n = n.Next;

				if (n.Value.Type != LexerTokenType.Value)
					throw new CifFormatException(n.Value.Pos, "Direction Y vector expected.");
				dy = n.Value.Value;
				n = n.Next;
			}

			BoxCommandDefinition b = new BoxCommandDefinition(me.Value.Pos, l, w, new Point(cx, cy), new Point(dx, dy));

			ParseEOT(n);

			return b;
		}

		protected static ParserToken ParseRoundCommand(LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "R")
				throw new CifFormatException(n.Value.Pos, "Round command expected.");
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Diameter expected.");
			long d = n.Value.Value;
			if (d < 0)
				throw new CifFormatException(n.Value.Pos, "Positive diameter expected.");
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Center X coordinate expected.");
			long x = n.Value.Value;
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Center Y coordinate expected.");
			long y = n.Value.Value;
			n = n.Next;

			RoundCommandDefinition r = new RoundCommandDefinition(me.Value.Pos, d, new Point(x, y));

			ParseEOT(n);

			return r;
		}

		protected static ParserToken ParseWireCommand(LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "W")
				throw new CifFormatException(n.Value.Pos, "Wire command expected.");
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Width expected.");
			long width = n.Value.Value;
			if (width < 0)
				throw new CifFormatException(n.Value.Pos, "Positive width expected.");
			n = n.Next;

			WireCommandDefinition wire = new WireCommandDefinition(me.Value.Pos, width);

			while (n.Value.Type == LexerTokenType.Value)
			{
				long x = n.Value.Value;
				n = n.Next;

				if (n.Value.Type != LexerTokenType.Value)
					throw new CifFormatException(n.Value.Pos, "Y coordinate expected.");
				long y = n.Value.Value;
				n = n.Next;

				wire.Points.Add(new Point(x, y));
			}

			ParseEOT(n);

			return wire;
		}

		protected static ParserToken ParseLayerCommand(LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "L")
				throw new CifFormatException(n.Value.Pos, "Layer command expected.");
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Name)
				throw new CifFormatException(n.Value.Pos, "Layer name expected.");
			string layer = n.Value.String;
			n = n.Next;

			ParseEOT(n);

			return new LayerCommandDefinition(me.Value.Pos, layer);
		}

		protected static ParserToken ParseDefStartCommand(LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "D" ||
			    n.Next.Value.Type != LexerTokenType.Name || n.Next.Value.String != "S")
				throw new CifFormatException(n.Value.Pos, "Definition Start command expected.");
			n = n.Next.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Symbol# expected.");
			long sym = n.Value.Value;
			if (sym < 0)
				throw new CifFormatException(n.Value.Pos, "Positive symbol# expected.");
			n = n.Next;

			long a = 1;
			long b = 1;
			if (n.Value.Type == LexerTokenType.Value)
			{
				a = n.Value.Value;
				if (a < 0)
					throw new CifFormatException(n.Value.Pos, "Positive factor expected.");
				n = n.Next;

				if (n.Value.Type != LexerTokenType.Value)
					throw new CifFormatException(n.Value.Pos, "Factor/divider B expected.");
				b = n.Value.Value;
				if (b < 0)
					throw new CifFormatException(n.Value.Pos, "Positive factor expected.");
				n = n.Next;
			}

			ParseEOT(n);

			return new DefStartCommandDefinition(me.Value.Pos, (uint)sym, a, b);
		}

		protected static ParserToken ParseDefFinishCommand(LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "D" ||
			    n.Next.Value.Type != LexerTokenType.Name || n.Next.Value.String != "F")
				throw new CifFormatException(n.Value.Pos, "Definition Finish command expected.");
			n = n.Next.Next;

			ParseEOT(n);

			return new DefFinishCommandDefinition(me.Value.Pos);
		}

		protected static ParserToken ParseDefDeleteCommand(LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "D" ||
			    n.Next.Value.Type != LexerTokenType.Name || n.Next.Value.String != "D")
				throw new CifFormatException(n.Value.Pos, "Definition Delete command expected.");
			n = n.Next.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Symbol# expected.");
			long sym = n.Value.Value;
			if (sym < 0)
				throw new CifFormatException(n.Value.Pos, "Positive symbol# expected.");
			n = n.Next;

			ParseEOT(n);

			return new DefDeleteCommandDefinition(me.Value.Pos, (uint)sym);
		}

		protected static ParserToken ParseDefinitionCommand(LinkedListNode<LexerToken> n)
		{
			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "D")
				throw new CifFormatException(n.Value.Pos, "Definition command expected.");

			if (n.Next.Value.Type == LexerTokenType.Name)
			{
				switch (n.Next.Value.String)
				{
				case "S":
					return ParseDefStartCommand(n);
				case "F":
					return ParseDefFinishCommand(n);
				case "D":
					return ParseDefDeleteCommand(n);
				}
			}

			throw new CifFormatException(n.Value.Pos, "Definition command DS, DF or DD expected.");
		}

		protected static ParserToken ParseCallCommand(LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "C")
				throw new CifFormatException(n.Value.Pos, "Call command expected.");
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Value)
				throw new CifFormatException(n.Value.Pos, "Symbol# expected.");
			long sym = n.Value.Value;
			if (sym < 0)
				throw new CifFormatException(n.Value.Pos, "Positive symbol# expected.");
			n = n.Next;

			CallCommandDefinition c = new CallCommandDefinition(me.Value.Pos, (uint)sym);

			c.Transformations.AddRange(ParseTransformations(ref n));

			ParseEOT(n);

			return c;
		}

		protected static ParserToken ParsePrintCommand(LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "1")
				throw new CifFormatException(n.Value.Pos, "Comment/Print command (1) expected.");
			n = n.Next;

			if (n.Value.Type != LexerTokenType.String)
				throw new CifFormatException(n.Value.Pos, "String literal expected.");
			string str = n.Value.String;
			n = n.Next;

			ParseEOT(n);

			return new PrintCommandDefinition(me.Value.Pos, str);
		}

		protected static ParserToken ParseTextCommand(LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "2")
				throw new CifFormatException(n.Value.Pos, "Text On Plot command (2) expected.");
			n = n.Next;

			if (n.Value.Type != LexerTokenType.String)
				throw new CifFormatException(n.Value.Pos, "String literal expected.");
			string str = n.Value.String;
			n = n.Next;

			TextCommandDefinition t = new TextCommandDefinition(me.Value.Pos, str);

			t.Transformations.AddRange(ParseTransformations(ref n));

			ParseEOT(n);

			return t;
		}

		protected static ParserToken ParseSymbolNameCommand(LinkedListNode<LexerToken> n)
		{
			LinkedListNode<LexerToken> me = n;

			if (n.Value.Type != LexerTokenType.Name || n.Value.String != "9")
				throw new CifFormatException(n.Value.Pos, "Symbol Name command (9) expected.");
			n = n.Next;

			if (n.Value.Type != LexerTokenType.Name)
				throw new CifFormatException(n.Value.Pos, "Symbol name expected.");
			string str = n.Value.String;
			n = n.Next;

			ParseEOT(n);

			return new SymbolNameCommandDefinition(me.Value.Pos, str);
		}

		protected IList<ParserToken> Parse(LinkedListNode<LexerToken> n)
		{
			switch (n.Value.Type)
			{
			case LexerTokenType.EOT:
				ParseEOT(n);
				return new ParserToken[] { };
			case LexerTokenType.Name:
				switch (n.Value.String)
				{
				case "P":
					return new ParserToken[] { ParsePolygonCommand(n) };
				case "B":
					return new ParserToken[] { ParseBoxCommand(n) };
				case "R":
					return new ParserToken[] { ParseRoundCommand(n) };
				case "W":
					return new ParserToken[] { ParseWireCommand(n) };
				case "L":
					return new ParserToken[] { ParseLayerCommand(n) };
				case "D":
					return new ParserToken[] { ParseDefinitionCommand(n) };
				case "C":
					return new ParserToken[] { ParseCallCommand(n) };
				case "1":
					return new ParserToken[] { ParsePrintCommand(n) };
				case "2":
					return new ParserToken[] { ParseTextCommand(n) };
				case "9":
					return new ParserToken[] { ParseSymbolNameCommand(n) };
				default:
					foreach (char x in n.Value.String)
						if (!char.IsDigit(x))
							throw new CifFormatException(n.Value.Pos, "Invalid command.");
					Console.Error.WriteLine("Warning: " + n.Value.Pos + ": Ignoring unknown user extension command " +
					                        n.Value.String + ".");
					return new ParserToken[] { };
				}
			}

			throw new CifFormatException(n.Value.Pos, "Unexpected token encountered by top level parser.");
		}

		private string file;
		private int    pos     = 0;
		private int    lineNum = 1;
		private LinkedList<LexerToken> fifo = new LinkedList<LexerToken>();

		private LinkedListNode<LexerToken> DequeueStatement()
		{
			bool hasEOT = false;
			foreach (LexerToken t in fifo)
				if (t.Type == LexerTokenType.EOT)
					hasEOT = true;
			if (!hasEOT)
				return null;

			LinkedList<LexerToken> l = new LinkedList<LexerToken>();
			while (true)
			{
				LinkedListNode<LexerToken> n = fifo.First;
				LexerToken                 t = n.Value;
				fifo.RemoveFirst();
				l.AddLast(t);
				if (t.Type == LexerTokenType.EOT)
					break;
			}

			return l.First;
		}

		public void WriteLine(string line)
		{
			LinkedListNode<LexerToken> lex = Lex(line, file, lineNum, ref pos);
			lineNum++;
			for (LinkedListNode<LexerToken> n = lex; n != null; n = n.Next)
				fifo.AddLast(n.Value);

			LinkedListNode<LexerToken> command;
			while ((command = DequeueStatement()) != null)
				foreach (ParserToken t in Parse(command))
					Commands.Add((CommandDefinition)t);
		}

		public override void NextFile(string file)
		{
			base.NextFile(file);

			if (fifo.Count != 0)
			{
				LexerToken t = fifo.First.Value;
				throw new CifFormatException(t.Pos, "End of file expected.");
			}

			this.file = file;
			pos       = 0;
			lineNum   = 1;
		}

		public void Flush()
		{
			NextFile(null);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var x in Commands)
				sb.AppendLine(x.ToString());
			return sb.ToString();
		}

		protected static Vector ApplyTrans(Point p, List<object> trans)
		{
			Vector v = new Vector((double)p.X, (double)p.Y);
			foreach (var t in trans)
			{
				if (t is double)
				{
					v *= (double)t;
				}
				else if (t is TranslationDefinition)
				{
					v.X += (double)((TranslationDefinition)t).Point.X;
					v.Y += (double)((TranslationDefinition)t).Point.Y;
				}
				else if (t is MirrorDefinition)
				{
					switch (((MirrorDefinition)t).Axis)
					{
						case MirrorAxis.X: v.X *= -1.0; break;
						case MirrorAxis.Y: v.Y *= -1.0; break;
					}
				}
				else if (t is RotationDefinition)
				{
					RotationDefinition r = (RotationDefinition)t;
					if (r.Direction.X != 0 && r.Direction.Y != 0)
					{
						// TODO: Handle non-90-degree rotation
						Console.Error.WriteLine("Warning: " + r.Pos + ": Rotation other that 0, 90, 180 or 270 not implemented yet.");
					}
					if (r.Direction.Y == 0)
					{
						if (r.Direction.X < 0)
							v = -v;
					}
					else
					{
						if (r.Direction.Y < 0)
							v = new Vector(v.Y, -v.X);
						else
							v = new Vector(-v.Y, v.X);
					}
				}
			}
			return v;
		}

		protected static double ApplyScale(long l, List<object> trans)
		{
			double d = (double)l;
			foreach (var t in trans)
				if (t is double)
					d *= (double)t;
			return d;
		}

		public Dictionary<string, List<IDrawable>> Interpret()
		{
			Dictionary<uint, Symbol> syms = new Dictionary<uint, Symbol>();
			Dictionary<string, List<IDrawable>> layers = new Dictionary<string, List<IDrawable>>();
			Interpret(Commands, false, syms, new List<object>(), layers, 0);
			return layers;
		}

		protected static void AddToLayer(Dictionary<string, List<IDrawable>> layers, string layer, IDrawable d)
		{
			string l = null;
			switch (layer)
			{
				case "CWG": l = "well";              break;
				case "CWP": l = "p-well";            break;
				case "CW":  l = "p-well";            break;
				case "CWN": l = "n-well";            break;
				case "CAA": l = "active";            break; // aka. diffusion
				case "CD":  l = "active";            break;
				case "CAP": l = "p-active";          break;
				case "CAN": l = "n-active";          break;
				case "ND":  l = "active";            break;
				case "CSG": l = "select";            break;
				case "CSP": l = "p-select";          break;
				case "CS":  l = "p-select";          break;
				case "CSN": l = "n-select";          break;
				case "CPG": l = "poly";              break;
				case "CP":  l = "poly";              break;
				case "NP":  l = "poly";              break;
				case "CCG": l = "contact";           break; // aka. cut
				case "CCC": l = "contact";           break;
				case "CC":  l = "contact";           break;
				case "NC":  l = "contact";           break;
				case "NO":  l = "oversize-contact";  break;
				case "CCA": l = "active-contact";    break;
				case "CCP": l = "poly-contact";      break;
				case "CCE": l = "electrode-contact"; break; // aka. poly2 contact
				case "CMF": l = "metal1";            break;
				case "CM":  l = "metal1";            break;
				case "NM":  l = "metal1";            break;
				case "CVA": l = "via1";              break;
				case "CMS": l = "metal2";            break;
				case "CVS": l = "via2";              break;
				case "CMT": l = "metal3";            break;
				case "CVT": l = "via3";              break;
				case "CMQ": l = "metal4";            break;
				case "CVQ": l = "via4";              break;
				case "CMP": l = "metal5";            break;
				case "CV5": l = "via5";              break;
				case "CM6": l = "metal6";            break;
				case "CVP": l = "p-high-voltage";    break;
				case "CVN": l = "n-high-voltage";    break;
				case "CTA": l = "thick-active";      break;
				case "COP": l = "mems-open";         break;
				case "CPS": l = "mems-etch-stop";    break;
				case "CX":  l = "pad";               break;
				case "XP":  l = "pad";               break;
				case "CFI": l = "exp-field-impl";    break;
				case "CPC": l = "poly-cap";          break;
				case "CSB": l = "silicide-block";    break;
				case "COG": l = "passivation";       break; // aka. glass
				case "CG":  l = "passivation";       break;
				case "NG":  l = "passivation";       break;
				case "CEL": l = "electrode";         break; // aka. poly2
				case "CCD": l = "buried";            break;
				case "NB":  l = "buried";            break; // contact between active and poly (opening in isolation between them)
				case "CBA": l = "p-base";            break; // base of NPN transistor
				case "CWC": l = "cap-well";          break;
				case "NI":  l = "implant";           break; // aka. depletion
				case "NJ":  l = "light-implant";     break;
				case "NE":  l = "hard-enhancement";  break;
				case "NF":  l = "light-enhancement"; break;
				case "CHR": l = "hi-res";            break;
			}
			if (l == null)
			{
				Console.Error.WriteLine("Warning: Layer " + layer + " not supported.");
				return;
			}
			if (!layers.ContainsKey(l))
				layers.Add(l, new List<IDrawable>());
			layers[l].Add(d);
		}

		protected static void Interpret(List<CommandDefinition>             cmds,
		                                bool                                isSub,
		                                Dictionary<uint, Symbol>            syms,
		                                List<object>                        trans,
		                                Dictionary<string, List<IDrawable>> layers,
		                                uint                                callDepth)
		{
			string layer = "ZZZZ";
			Symbol curSym = null;

			if (callDepth >= 256)
				throw new ApplicationException("Symbol call depth >256.");

			foreach (CommandDefinition cmd in cmds)
			{
				if (cmd is PolygonCommandDefinition)
				{
					PolygonCommandDefinition pcmd = (PolygonCommandDefinition)cmd;
					if (curSym != null)
					{
						curSym.Commands.Add(pcmd);
					}
					else
					{
						Polygon pol = new Polygon();
						foreach (var p in pcmd.Points)
							pol.P.Add(ApplyTrans(p, trans));
						AddToLayer(layers, layer, pol);
					}
				}
				else if (cmd is BoxCommandDefinition)
				{
					BoxCommandDefinition bcmd = (BoxCommandDefinition)cmd;
					if (curSym != null)
					{
						curSym.Commands.Add(bcmd);
					}
					else
					{
						if (bcmd.Direction.X != 0 && bcmd.Direction.Y != 0)
						{
							// TODO: Handle non-90-degree rotation
							Console.Error.WriteLine("Warning: " + bcmd.Pos + ": Boxes rotated by angles other than 0, 90, 180 or 270 not implemented yet.");
						}
						else
						{
							Vector c = ApplyTrans(bcmd.Center, trans);
							double w = ApplyScale(bcmd.Length, trans); // Definition of length and width in CIF is seriously fucked up.
							double h = ApplyScale(bcmd.Width, trans);  //
							if (bcmd.Direction.Y != 0)
								(w, h) = (h, w);
							Box b = new Box(new Vector(c.X - w / 2.0, c.Y - h / 2.0),
							                new Vector(c.X + w / 2.0, c.Y + h / 2.0));
							AddToLayer(layers, layer, b);
						}
					}
				}
				else if (cmd is RoundCommandDefinition)
				{
					RoundCommandDefinition rcmd = (RoundCommandDefinition)cmd;
					if (curSym != null)
					{
						curSym.Commands.Add(rcmd);
					}
					else
					{
						Vector c = ApplyTrans(rcmd.Center, trans);
						double d = ApplyScale(rcmd.Diameter, trans);
						Round r = new Round(c, d / 2.0);
						AddToLayer(layers, layer, r);
					}
				}
				else if (cmd is WireCommandDefinition)
				{
					WireCommandDefinition wcmd = (WireCommandDefinition)cmd;
					// TODO: Handle wires
					Console.Error.WriteLine("Warning: " + wcmd.Pos + ": Wires not implemented yet.");
				}
				else if (cmd is LayerCommandDefinition)
				{
					LayerCommandDefinition lcmd = (LayerCommandDefinition)cmd;
					if (curSym != null)
						curSym.Commands.Add(lcmd);
					else
						layer = lcmd.Layer;
				}
				else if (cmd is DefStartCommandDefinition)
				{
					DefStartCommandDefinition scmd = (DefStartCommandDefinition)cmd;
					if (curSym != null)
						throw new CifFormatException(scmd.Pos, "Nesting symbol definitions is illegal.");
					if (syms.ContainsKey(scmd.Symbol))
					{
						Console.Error.WriteLine("Warning: " + scmd.Pos + ": Symbol re-definition without prior deletion.");
						syms.Remove(scmd.Symbol);
					}
					Symbol s = new Symbol(scmd.Symbol, scmd.A, scmd.B);
					syms.Add(scmd.Symbol, s);
					curSym = s;
				}
				else if (cmd is DefFinishCommandDefinition)
				{
					DefFinishCommandDefinition fcmd = (DefFinishCommandDefinition)cmd;
					if (curSym == null)
						throw new CifFormatException(fcmd.Pos, "DF without DS found.");
					curSym = null;
				}
				else if (cmd is DefDeleteCommandDefinition)
				{
					DefDeleteCommandDefinition dcmd = (DefDeleteCommandDefinition)cmd;
					if (curSym != null)
						throw new CifFormatException(dcmd.Pos, "Deleting symbols from within symbol definition is illegal.");
					// TODO: Handle symbol deletion
					Console.Error.WriteLine("Warning: " + dcmd.Pos + ": DD command not implemented yet.");
				}
				else if (cmd is CallCommandDefinition)
				{
					CallCommandDefinition ccmd = (CallCommandDefinition)cmd;
					if (!syms.ContainsKey(ccmd.Symbol))
						throw new CifFormatException(ccmd.Pos, "Calling non-existent symbol.");
					List<object> subTrans = new List<object>();
					Symbol callSym = syms[ccmd.Symbol];
					if (callSym.A != callSym.B)
						subTrans.Add((double)callSym.A / (double)callSym.B);
					subTrans.AddRange(ccmd.Transformations);
					subTrans.AddRange(trans);
					Interpret(callSym.Commands, true, syms, subTrans, layers, callDepth + 1);
				}
				else if (cmd is PrintCommandDefinition)
				{
					PrintCommandDefinition pcmd = (PrintCommandDefinition)cmd;
					Console.Error.WriteLine(pcmd.Text);
				}
				else if (cmd is TextCommandDefinition)
				{
					TextCommandDefinition tcmd = (TextCommandDefinition)cmd;
					// TODO: Handle texts
					Console.Error.WriteLine("Warning: " + tcmd.Pos + ": Texts on Plot not implemented yet.");
				}
				else if (cmd is SymbolNameCommandDefinition)
				{
					SymbolNameCommandDefinition ncmd = (SymbolNameCommandDefinition)cmd;
					if (curSym == null)
					{
						Console.Error.WriteLine("Warning: " + ncmd.Pos + ": Symbol name definition outside of symbol definition.");
						continue;
					}
					if (curSym.Name != null)
						Console.Error.WriteLine("Warning: " + ncmd.Pos + ": Symbol name re-definition.");
					curSym.Name = ncmd.Text;
				}
			}
		}
	}
}
