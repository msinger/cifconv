using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace cifconv
{
	class MainClass
	{
		public static int Main(string[] args)
		{
			bool   parseOptions = true;
			bool   genPng       = false;
			bool   widthGiven   = false;
			bool   heightGiven  = false;
			bool   roundGrowing = false;
			string outPng       = null;
			string layer        = null;
			string scaleStr     = null;
			string originXStr   = null;
			string originYStr   = null;
			string widthStr     = null;
			string heightStr    = null;
			string bgStr        = null;
			string atStr        = null;
			List<string> files  = new List<string>();

			for (int i = 0; i < args.Length; i++)
			{
				if (parseOptions && args[i].StartsWith("--"))
				{
					string nextArg = (i + 1 < args.Length) ? args[i + 1] : null;
					switch (args[i])
					{
						case "--png":        genPng       = true; outPng       = nextArg; i++; break;
						case "--layer":                           layer        = nextArg; i++; break;
						case "--scale":                           scaleStr     = nextArg; i++; break;
						case "--origin-x":                        originXStr   = nextArg; i++; break;
						case "--origin-y":                        originYStr   = nextArg; i++; break;
						case "--width":                           widthStr     = nextArg; i++; break;
						case "--height":                          heightStr    = nextArg; i++; break;
						case "--bg":                              bgStr        = nextArg; i++; break;
						case "--at":                              atStr        = nextArg; i++; break;
						case "--roundgrowing":                    roundGrowing = true;         break;
						case "--":           parseOptions = false;                             break;
						default:             PrintHelp(); return args[i] == "--help" ? 0 : 1;
					}
					continue;
				}

				files.Add(args[i]);
			}

			if (!string.IsNullOrEmpty(layer) && !LayerIsValid(layer))
			{
				Console.Error.WriteLine("Invalid layer: " + layer);
				return 1;
			}

			double scale = double.NaN;
			if (scaleStr != null && scaleStr != "auto")
				scale = double.Parse(scaleStr, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo);

			double originX = double.NaN;
			if (originXStr != null && originXStr != "auto")
				originX = double.Parse(originXStr, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo);
			double originY = double.NaN;
			if (originYStr != null && originYStr != "auto")
				originY = double.Parse(originYStr, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo);

			int width = 0;
			if (widthStr != null && widthStr != "auto")
				width = int.Parse(widthStr, NumberStyles.None, NumberFormatInfo.InvariantInfo);
			int height = 0;
			if (heightStr != null && heightStr != "auto")
				height = int.Parse(heightStr, NumberStyles.None, NumberFormatInfo.InvariantInfo);
			if (width != 0)
				widthGiven = true;
			if (height != 0)
				heightGiven = true;

			uint bg = 0;
			if (bgStr != null)
				bg = uint.Parse(bgStr, NumberStyles.AllowHexSpecifier, NumberFormatInfo.InvariantInfo);

			double atX = double.NaN;
			double atY = double.NaN;
			bool atPng = false;
			string atLayer = null;
			if (atStr != null)
			{
				string[] atArr = atStr.Split(new char[] { ';', ',', 'x', 'X', '*' }, 2);
				if (atArr.Length != 2)
				{
					Console.Error.WriteLine("Invalid parameter to --at option.");
					return 1;
				}
				if (atArr[0].StartsWith("png"))
				{
					atPng = true;
					atArr[0] = atArr[0].Substring(3);
				}
				var atIdx = atArr[1].IndexOf('@');
				if (atIdx != -1)
				{
					atLayer = atArr[1].Substring(atIdx + 1);
					atArr[1] = atArr[1].Substring(0, atIdx);
					if (!LayerIsValid(atLayer))
					{
						Console.Error.WriteLine("Invalid layer: " + layer);
						return 1;
					}
				}
				atX = double.Parse(atArr[0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo);
				atY = double.Parse(atArr[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, NumberFormatInfo.InvariantInfo);
			}

			if (files.Count == 0)
				files.Add("-");

			Cif cif = new Cif();

			foreach (string fn in files)
			{
				cif.NextFile((files.Count != 1 || fn != "-") ? fn : null);

				TextReader f = Console.In;
				if (fn != "-")
					f = File.OpenText(fn);

				string l;
				while ((l = f.ReadLine()) != null)
					cif.WriteLine(l);
			}
			cif.Flush();
			Layout layout = new Layout(cif.Interpret());

			if (layout.HasNonIntegerCoords())
			{
				Console.Error.WriteLine("WARNING: Converting boxes from center-size to p0-p1 coords resulted");
				Console.Error.WriteLine("         in non-integer coordinates. The scale of the design was probably");
				Console.Error.WriteLine("         sub micron. Increase the scale before exporting to avoid rounding");
				Console.Error.WriteLine("         errors and possibly unintended gaps or misalignments.");
			}

			if (roundGrowing)
			{
				Box.RoundGrowing = true;
				Round.RoundGrowing = true;
			}

			Box aabb = layout.GetAABB();
			Console.Error.WriteLine("Layout bounding box: " + aabb.ToString());

			double bbWidth  = aabb.P1.X - aabb.P0.X;
			double bbHeight = aabb.P1.Y - aabb.P0.Y;
			if (!widthGiven)
				width = (int)(bbWidth * (!double.IsNaN(scale) ? scale : 1.0));
			if (!heightGiven)
				height = (int)(bbHeight * (!double.IsNaN(scale) ? scale : 1.0));
			if (double.IsNaN(scale))
			{
				double bbwdh = bbWidth / bbHeight;
				double bbhdw = bbHeight / bbWidth;
				if (!widthGiven && double.IsFinite(bbhdw))
					width = (int)((double)height * bbwdh);
				if (!heightGiven && double.IsFinite(bbwdh))
					height = (int)((double)width * bbhdw);
			}
			if (width <= 0)
				width = 1;
			if (height <= 0)
				height = 1;
			double wdh = (double)width / (double)height;
			double hdw = (double)height / (double)width;
			if (genPng)
			{
				if (!widthGiven && width > 16384)
				{
					width = 16384;
					if (!heightGiven)
						height = (int)((double)width * hdw);
				}
				if (!heightGiven && height > 16384)
				{
					height = 16384;
					if (!widthGiven)
						width = (int)((double)height * wdh);
				}
			}

			if (double.IsNaN(scale))
			{
				if (widthGiven && bbWidth > 0.0)
					scale = (double)width / bbWidth;
				else if (heightGiven && bbHeight > 0.0)
					scale = (double)height / bbHeight;
				else if (genPng && bbWidth > 0.0)
					scale = (double)width / bbWidth;
				else if (genPng && bbHeight > 0.0)
					scale = (double)height / bbHeight;
				else
					scale = 1.0;

				if (!widthGiven)
					width = (int)(bbWidth * scale);
				if (!heightGiven)
					height = (int)(bbHeight * scale);
				if (width <= 0)
					width = 1;
				if (height <= 0)
					height = 1;
			}

			Console.Error.WriteLine("Apply scale: " + scale.ToString(CultureInfo.InvariantCulture));
			aabb.Scale(scale);
			Console.Error.WriteLine("Layout bounding box after scale: " + aabb.ToString());

			Console.Error.WriteLine("Image size: " + width.ToString(CultureInfo.InvariantCulture) +
			                        "x" + height.ToString(CultureInfo.InvariantCulture));

			if (double.IsNaN(originX))
				originX = aabb.P0.X;
			if (double.IsNaN(originY))
				originY = aabb.P1.Y;
			Console.Error.WriteLine("Set origin: " + originX.ToString(CultureInfo.InvariantCulture) +
			                        "x" + originY.ToString(CultureInfo.InvariantCulture));
			aabb.Translate(new Vector(-originX, -originY));
			aabb.MakeIntegerCoords();
			Console.Error.WriteLine("Layout bounding box after origin translation: " + aabb.ToString());

			Dictionary<string, List<int>> selected = new Dictionary<string, List<int>>();
			if (atStr != null)
			{
				Vector v = new Vector(atX, atY);
				if (atPng)
				{
					v.Y = -v.Y;
					v += new Vector(originX, originY);
					if (scale != 0)
						v /= scale;
				}
				Console.Error.WriteLine(v);
				selected = layout.GetShapesAtCoord(v, atLayer);
			}
			if (selected.Count != 0)
			{
				layout.Layers.Add("selected", new List<IDrawable>());
				foreach (var kvp in selected)
					foreach (var i in kvp.Value)
						layout.Layers["selected"].Add((IDrawable)layout.Layers[kvp.Key][i].Clone());
				Console.Error.WriteLine(layout.Layers["selected"].Count + " objects selected on " + selected.Count + " layers.");
			}

			layout.Scale(scale);
			layout.Translate(new Vector(-originX, -originY));
			layout.MakeIntegerCoords();

			if (!genPng)
				Console.Error.WriteLine("CIF parsed successfully.");

			if (genPng)
			{
				Stream s = Console.OpenStandardOutput();
				if (!string.IsNullOrEmpty(outPng) && outPng != "-")
					s = File.Create(outPng);
				GenPngLayers(s, layout, layer, width, height, bg);
				s.Flush();
			}

			return 0;
		}

		private static void PrintHelp()
		{
			Console.Error.WriteLine("Usage: cifconv.exe [<OPTIONS>] [<FILES>]");
			Console.Error.WriteLine();
			Console.Error.WriteLine("OPTIONS:");
			Console.Error.WriteLine("  --png <FILE>                 Convert CIF to PNG containing everything.");
			Console.Error.WriteLine("  --layer <LAYER>              Select one layer to operate on. If not given,");
			Console.Error.WriteLine("                               all layers are selected.");
			Console.Error.WriteLine("  --scale <FACTOR>             Choose scale factor for image.");
			Console.Error.WriteLine("  --origin-x <XCOORD>          Choose X translation of origin point of image.");
			Console.Error.WriteLine("  --origin-y <YCOORD>          Choose Y translation of origin point of image.");
			Console.Error.WriteLine("  --width <XSIZE>              Choose width of image in pixels.");
			Console.Error.WriteLine("  --height <YSIZE>             Choose height of image in pixels.");
			Console.Error.WriteLine("  --bg <AARRGGBB>              Choose PNG background color. 00000000 is default.");
			Console.Error.WriteLine("  --at [png]<COORD>[@<LAYER>]  Highlight object(s) at coordinate at optionally");
			Console.Error.WriteLine("                               given layer. If prefixed by 'png', coordinates");
			Console.Error.WriteLine("                               are given in the transformed final image space.");
			Console.Error.WriteLine("  --roundgrowing               Use alternative rounding style. May fix");
			Console.Error.WriteLine("                               unintentional gaps between objects.");
			Console.Error.WriteLine();
			Console.Error.WriteLine("Without any output option, cifconv.exe just reads CIF");
			Console.Error.WriteLine("and checks if there are no errors.");
		}

		private static void GenPngLayers(Stream s, Layout layout, string layer, int width, int height, uint bg)
		{
			if (string.IsNullOrEmpty(layer))
			{
				string[] order = new string[] {
					"silicide-block",
					"contact",
					"via1",
					"via2",
					"via3",
					"via4",
					"via5",
					"pad",
					"hi-res",
					"select",
					"n-select",
					"p-high-voltage",
					"n-high-voltage",
					"thick-active",
					"passivation",
					"poly-cap",
					"well",
					"n-well",
					"p-well",
					"p-base",
					"p-select",
					"electrode",
					"metal4",
					"metal5",
					"metal6",
					"selected",
				};

				Bitmap bmp = BitmapFromLayer(layout, "active", width, height);
				var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
				                        ImageLockMode.ReadWrite, bmp.PixelFormat);
				using (Bitmap bmp2 = BitmapFromLayer(layout, "poly", width, height))
				{
					var data2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height),
					                          ImageLockMode.ReadOnly, bmp2.PixelFormat);
					unsafe
					{
						Argb* p = (Argb*)data.Scan0;
						Argb* q = (Argb*)data2.Scan0;
						for (int i = 0; i < data.Stride * data.Height / sizeof(Argb); i++)
						{
							if (q[i].A != 0)
							{
								// Draw poly over active?
								if (p[i].A != 0)
									p[i] = new Argb(0xff, 0x94, 0xa9, 0x77);
								else
									p[i] = q[i];
							}
						}
					}
					bmp2.UnlockBits(data2);
				}
				using (Bitmap bmp2 = BitmapFromLayer(layout, "metal1", width, height))
				{
					var data2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height),
					                          ImageLockMode.ReadOnly, bmp2.PixelFormat);
					unsafe
					{
						Argb* p = (Argb*)data.Scan0;
						Argb* q = (Argb*)data2.Scan0;
						for (int i = 0; i < data.Stride * data.Height / sizeof(Argb); i++)
						{
							if (q[i].A != 0)
							{
								switch (p[i].R)
								{
									// Draw metal1 over...
									case /* active  */ 0x6b: p[i] = new Argb(0xff, 0x59, 0xbe, 0x90); break;
									case /* poly    */ 0xff: p[i] = new Argb(0xff, 0x83, 0x89, 0xa9); break;
									case /* act+pol */ 0x94: p[i] = new Argb(0xff, 0x77, 0xb4, 0x85); break;
									default:                 p[i] = q[i];                             break;
								}
							}
						}
					}
					bmp2.UnlockBits(data2);
				}
				using (Bitmap bmp2 = BitmapFromLayer(layout, "metal2", width, height))
				{
					var data2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height),
					                          ImageLockMode.ReadOnly, bmp2.PixelFormat);
					unsafe
					{
						Argb* p = (Argb*)data.Scan0;
						Argb* q = (Argb*)data2.Scan0;
						for (int i = 0; i < data.Stride * data.Height / sizeof(Argb); i++)
						{
							if (q[i].A != 0)
							{
								switch (p[i].R)
								{
									// Draw metal2 over...
									case /* active     */ 0x6b: p[i] = new Argb(0xff, 0x8d, 0x98, 0x93); break;
									case /* poly       */ 0xff: p[i] = new Argb(0xff, 0xae, 0x5a, 0xa2); break;
									case /* act+pol    */ 0x94: p[i] = new Argb(0xff, 0x9f, 0x7a, 0x9c); break;
									case /* metal1     */ 0x60: p[i] = new Argb(0xff, 0x78, 0x73, 0xc0); break;
									case /* act+m1     */ 0x59: p[i] = new Argb(0xff, 0x83, 0x87, 0xab); break;
									case /* pol+m1     */ 0x83: p[i] = new Argb(0xff, 0x94, 0x68, 0xb3); break;
									case /* act+pol+m1 */ 0x77: p[i] = new Argb(0xff, 0x91, 0x81, 0xa4); break;
									default:                    p[i] = q[i];                             break;
								}
							}
						}
					}
					bmp2.UnlockBits(data2);
				}
				using (Bitmap bmp2 = BitmapFromLayer(layout, "metal3", width, height))
				{
					var data2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height),
					                          ImageLockMode.ReadOnly, bmp2.PixelFormat);
					unsafe
					{
						Argb* p = (Argb*)data.Scan0;
						Argb* q = (Argb*)data2.Scan0;
						for (int i = 0; i < data.Stride * data.Height / sizeof(Argb); i++)
						{
							if (q[i].A != 0)
							{
								switch (p[i].G)
								{
									// Draw metal3 over...
									case /* active        */ 0xe2: p[i] = new Argb(0xff, 0x8f, 0xcb, 0x36); break;
									case /* poly          */ 0x9b: p[i] = new Argb(0xff, 0xbc, 0x98, 0x4f); break;
									case /* act+pol       */ 0xa9: p[i] = new Argb(0xff, 0xa7, 0xb3, 0x44); break;
									case /* metal1        */ 0xd1: p[i] = new Argb(0xff, 0x88, 0xb8, 0x6f); break;
									case /* act+m1        */ 0xbe: p[i] = new Argb(0xff, 0x8c, 0xc3, 0x53); break;
									case /* pol+m1        */ 0x89: p[i] = new Argb(0xff, 0xa4, 0xa8, 0x61); break;
									case /* act+pol+m1    */ 0xb4: p[i] = new Argb(0xff, 0x9a, 0xbb, 0x4c); break;
									case /* metal2        */ 0x5f: p[i] = new Argb(0xff, 0xb9, 0x88, 0x6c); break;
									case /* act+m2        */ 0x98: p[i] = new Argb(0xff, 0xa6, 0xad, 0x54); break;
									case /* pol+m2        */ 0x5a: p[i] = new Argb(0xff, 0xbb, 0x90, 0x5e); break;
									case /* act+pol+m2    */ 0x7a: p[i] = new Argb(0xff, 0xb1, 0x9f, 0x59); break;
									case /* m1+m2         */ 0x73: p[i] = new Argb(0xff, 0xa2, 0xa1, 0x70); break;
									case /* act+m1+m2     */ 0x87: p[i] = new Argb(0xff, 0xa4, 0xa8, 0x62); break;
									case /* pol+m1+m2     */ 0x68: p[i] = new Argb(0xff, 0xaf, 0x99, 0x67); break;
									case /* act+pol+m1+m2 */ 0x81: p[i] = new Argb(0xff, 0xaa, 0xa3, 0x5e); break;
									default:                       p[i] = q[i];                             break;
								}
							}
						}
					}
					bmp2.UnlockBits(data2);
				}
				unsafe
				{
					uint* p = (uint*)data.Scan0;
					for (int i = 0; i < data.Stride * data.Height / sizeof(uint); i++)
						if (p[i] == 0)
							p[i] = bg;
				}
				bmp.UnlockBits(data);
				using (Graphics g = Graphics.FromImage(bmp))
				{
					g.ScaleTransform(1.0f, -1.0f);
					foreach (var l in order)
					{
						if (layout.Layers.ContainsKey(l))
						{
							Pen   p = GetLayerPen(l);
							Brush b = GetLayerBrush(l);
							foreach (var d in layout.Layers[l])
								d.Draw(g, p, b);
						}
					}
				}
				bmp.Save(s, ImageFormat.Png);
			}
			else
			{
				Pen   p   = GetLayerPen(layer);
				Brush b   = GetLayerBrush(layer);
				Brush bgb = new SolidBrush(Color.FromArgb(unchecked((int)bg)));
				List<IDrawable> l = new List<IDrawable>();
				if (layout.Layers.ContainsKey(layer))
					l = layout.Layers[layer];
				else
					Console.Error.WriteLine("Layer " + layer + " is empty.");
				Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				using (Graphics g = Graphics.FromImage(bmp))
				{
					g.FillRectangle(bgb, new Rectangle(0, 0, width, height));
					g.ScaleTransform(1.0f, -1.0f);
					foreach (var d in l)
						d.Draw(g, p, b);
				}
				bmp.Save(s, ImageFormat.Png);
			}
		}

		private static Bitmap BitmapFromLayer(Layout layout, string layer, int width, int height)
		{
			Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.ScaleTransform(1.0f, -1.0f);
				if (layout.Layers.ContainsKey(layer))
				{
					var   l = layout.Layers[layer];
					Color c = GetLayerColor(layer);
					Pen   p = new Pen(c);
					Brush b = new SolidBrush(c);
					foreach (var d in l)
						d.Draw(g, p, b);
				}
			}
			return bmp;
		}

		private static bool LayerIsValid(string layer)
		{
			switch (layer)
			{
				case "well":
				case "p-well":
				case "n-well":
				case "active":
				case "select":
				case "p-select":
				case "n-select":
				case "poly":
				case "contact":
				case "metal1":
				case "via1":
				case "metal2":
				case "via2":
				case "metal3":
				case "via3":
				case "metal4":
				case "via4":
				case "metal5":
				case "via5":
				case "metal6":
				case "p-high-voltage":
				case "n-high-voltage":
				case "thick-active":
				case "mems-open":
				case "mems-etch-stop":
				case "pad":
				case "exp-field-impl":
				case "poly-cap":
				case "silicide-block":
				case "passivation":
				case "electrode":
				case "burried":
				case "p-base":
				case "cap-well":
				case "depletion":
				case "hi-res":         return true;
				default:               return false;
			}
		}

		private static Color GetLayerColor(string layer)
		{
			switch (layer)
			{
				case "well":           return Color.FromArgb(0xff, 0x8b, 0x63, 0x2e);
				case "p-well":         return Color.FromArgb(0xff, 0x8b, 0x63, 0x2e);
				case "n-well":         return Color.FromArgb(0xff, 0x8b, 0x63, 0x2e);
				case "active":         return Color.FromArgb(0xff, 0x6b, 0xe2, 0x60);
				case "select":         return Color.FromArgb(0xff, 0xff, 0xff, 0x00);
				case "p-select":       return Color.FromArgb(0xff, 0xff, 0xff, 0x00);
				case "n-select":       return Color.FromArgb(0xff, 0xff, 0xff, 0x00);
				case "poly":           return Color.FromArgb(0xff, 0xff, 0x9b, 0xc0);
				case "contact":        return Color.FromArgb(0xff, 0x64, 0x64, 0x64);
				case "metal1":         return Color.FromArgb(0xff, 0x60, 0xd1, 0xff);
				case "via1":           return Color.FromArgb(0xff, 0xb4, 0xb4, 0xb4);
				case "metal2":         return Color.FromArgb(0xff, 0xe0, 0x5f, 0xff);
				case "via2":           return Color.FromArgb(0xff, 0xb4, 0xb4, 0xb4);
				case "metal3":         return Color.FromArgb(0xff, 0xf7, 0xfb, 0x14);
				case "via3":           return Color.FromArgb(0xff, 0xb4, 0xb4, 0xb4);
				case "metal4":         return Color.FromArgb(0xff, 0x96, 0x96, 0xff);
				case "via4":           return Color.FromArgb(0xff, 0xb4, 0xb4, 0xb4);
				case "metal5":         return Color.FromArgb(0xff, 0xff, 0xbe, 0x06);
				case "via5":           return Color.FromArgb(0xff, 0xb4, 0xb4, 0xb4);
				case "metal6":         return Color.FromArgb(0xff, 0x00, 0xff, 0xff);
				case "p-high-voltage": return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "n-high-voltage": return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "thick-active":   return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "mems-open":      return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "mems-etch-stop": return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "pad":            return Color.FromArgb(0xff, 0xff, 0x00, 0x00);
				case "exp-field-impl": return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "poly-cap":       return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "silicide-block": return Color.FromArgb(0xff, 0xff, 0x9b, 0xc0);
				case "passivation":    return Color.FromArgb(0xff, 0x64, 0x64, 0x64); // aka. glass
				case "electrode":      return Color.FromArgb(0xff, 0xff, 0xbe, 0x06); // aka. poly2
				case "burried":        return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "p-base":         return Color.FromArgb(0xff, 0x6b, 0xe2, 0x60);
				case "cap-well":       return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "depletion":      return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "hi-res":         return Color.FromArgb(0xff, 0xff, 0x00, 0x00);
				case "selected":       return Color.FromArgb(0xaa, 0x00, 0xff, 0xff);
				default:               return Color.FromArgb(0x00, 0x00, 0x00, 0x00);
			}
		}

		private static Pen GetLayerPen(string layer)
		{
			switch (layer)
			{
			case "metal5":
			case "p-base":
			case "hi-res":
				Color c = GetLayerColor(layer);
				return new Pen(c);
			case "selected":
				c = GetLayerColor(layer);
				Pen p = new Pen(c, 5);
				p.DashStyle = DashStyle.DashDotDot;
				return p;
			default:
				return Pens.Transparent;
			}
		}

		private static Brush GetLayerBrush(string layer)
		{
			Color c = GetLayerColor(layer);
			bool texture = false;
			ushort[] b = new ushort[16];
			byte[] arr = new byte[16 * 16 * 4];
			switch (layer)
			{
			case "well":
			case "p-well":
				texture = true;
				b[15] = 0b0000001000000010;
				b[14] = 0b0000000100000001;
				b[13] = 0b1000000010000000;
				b[12] = 0b0100000001000000;
				b[11] = 0b0010000000100000;
				b[10] = 0b0001000000010000;
				b[9]  = 0b0000100000001000;
				b[8]  = 0b0000010000000100;
				b[7]  = 0b0000001000000010;
				b[6]  = 0b0000000100000001;
				b[5]  = 0b1000000010000000;
				b[4]  = 0b0100000001000000;
				b[3]  = 0b0010000000100000;
				b[2]  = 0b0001000000010000;
				b[1]  = 0b0000100000001000;
				b[0]  = 0b0000010000000100;
				break;
			case "n-well":
				texture = true;
				b[15] = 0b0000001000000010;
				b[14] = 0b0000000000000000;
				b[13] = 0b0010000000100000;
				b[12] = 0b0000000000000000;
				b[11] = 0b0000001000000010;
				b[10] = 0b0000000000000000;
				b[9]  = 0b0010000000100000;
				b[8]  = 0b0000000000000000;
				b[7]  = 0b0000001000000010;
				b[6]  = 0b0000000000000000;
				b[5]  = 0b0010000000100000;
				b[4]  = 0b0000000000000000;
				b[3]  = 0b0000001000000010;
				b[2]  = 0b0000000000000000;
				b[1]  = 0b0010000000100000;
				b[0]  = 0b0000000000000000;
				break;
			case "p-select":
				texture = true;
				b[15] = 0b0001000000010000;
				b[14] = 0b0010000000100000;
				b[13] = 0b0100000001000000;
				b[12] = 0b1000000010000000;
				b[11] = 0b0000000100000001;
				b[10] = 0b0000001000000010;
				b[9]  = 0b0000010000000100;
				b[8]  = 0b0000100000001000;
				b[7]  = 0b0001000000010000;
				b[6]  = 0b0010000000100000;
				b[5]  = 0b0100000001000000;
				b[4]  = 0b1000000010000000;
				b[3]  = 0b0000000100000001;
				b[2]  = 0b0000001000000010;
				b[1]  = 0b0000010000000100;
				b[0]  = 0b0000100000001000;
				break;
			case "select":
			case "n-select":
				texture = true;
				b[15] = 0b0000000100000001;
				b[14] = 0b0000000000000000;
				b[13] = 0b0001000000010000;
				b[12] = 0b0000000000000000;
				b[11] = 0b0000000100000001;
				b[10] = 0b0000000000000000;
				b[9]  = 0b0001000000010000;
				b[8]  = 0b0000000000000000;
				b[7]  = 0b0000000100000001;
				b[6]  = 0b0000000000000000;
				b[5]  = 0b0001000000010000;
				b[4]  = 0b0000000000000000;
				b[3]  = 0b0000000100000001;
				b[2]  = 0b0000000000000000;
				b[1]  = 0b0001000000010000;
				b[0]  = 0b0000000000000000;
				break;
			case "metal4":
				texture = true;
				b[15] = 0b1111111111111111;
				b[14] = 0b0000000000000000;
				b[13] = 0b1111111111111111;
				b[12] = 0b0000000000000000;
				b[11] = 0b1111111111111111;
				b[10] = 0b0000000000000000;
				b[9]  = 0b1111111111111111;
				b[8]  = 0b0000000000000000;
				b[7]  = 0b1111111111111111;
				b[6]  = 0b0000000000000000;
				b[5]  = 0b1111111111111111;
				b[4]  = 0b0000000000000000;
				b[3]  = 0b1111111111111111;
				b[2]  = 0b0000000000000000;
				b[1]  = 0b1111111111111111;
				b[0]  = 0b0000000000000000;
				break;
			case "metal5":
				texture = true;
				b[15] = 0b1000100010001000;
				b[14] = 0b0001000100010001;
				b[13] = 0b0010001000100010;
				b[12] = 0b0100010001000100;
				b[11] = 0b1000100010001000;
				b[10] = 0b0001000100010001;
				b[9]  = 0b0010001000100010;
				b[8]  = 0b0100010001000100;
				b[7]  = 0b1000100010001000;
				b[6]  = 0b0001000100010001;
				b[5]  = 0b0010001000100010;
				b[4]  = 0b0100010001000100;
				b[3]  = 0b1000100010001000;
				b[2]  = 0b0001000100010001;
				b[1]  = 0b0010001000100010;
				b[0]  = 0b0100010001000100;
				break;
			case "metal6":
				texture = true;
				b[15] = 0b1000100010001000;
				b[14] = 0b0100010001000100;
				b[13] = 0b0010001000100010;
				b[12] = 0b0001000100010001;
				b[11] = 0b1000100010001000;
				b[10] = 0b0100010001000100;
				b[9]  = 0b0010001000100010;
				b[8]  = 0b0001000100010001;
				b[7]  = 0b1000100010001000;
				b[6]  = 0b0100010001000100;
				b[5]  = 0b0010001000100010;
				b[4]  = 0b0001000100010001;
				b[3]  = 0b1000100010001000;
				b[2]  = 0b0100010001000100;
				b[1]  = 0b0010001000100010;
				b[0]  = 0b0001000100010001;
				break;
			case "p-high-voltage":
			case "n-high-voltage":
			case "thick-active":
				texture = true;
				b[15] = 0b0100000001000000;
				b[14] = 0b1000000010000000;
				b[13] = 0b0000000100000001;
				b[12] = 0b0000001000000010;
				b[11] = 0b0000000100000001;
				b[10] = 0b1000000010000000;
				b[9]  = 0b0100000001000000;
				b[8]  = 0b0010000000100000;
				b[7]  = 0b0100000001000000;
				b[6]  = 0b1000000010000000;
				b[5]  = 0b0000000100000001;
				b[4]  = 0b0000001000000010;
				b[3]  = 0b0000000100000001;
				b[2]  = 0b1000000010000000;
				b[1]  = 0b0100000001000000;
				b[0]  = 0b0010000000100000;
				break;
			case "silicide-block":
				texture = true;
				b[15] = 0b0001000000010000;
				b[14] = 0b0010100000101000;
				b[13] = 0b0100010001000100;
				b[12] = 0b1000001010000010;
				b[11] = 0b0000000100000001;
				b[10] = 0b0000000000000000;
				b[9]  = 0b0000000000000000;
				b[8]  = 0b0000000000000000;
				b[7]  = 0b0001000000010000;
				b[6]  = 0b0010100000101000;
				b[5]  = 0b0100010001000100;
				b[4]  = 0b1000001010000010;
				b[3]  = 0b0000000100000001;
				b[2]  = 0b0000000000000000;
				b[1]  = 0b0000000000000000;
				b[0]  = 0b0000000000000000;
				break;
			case "passivation": // aka. glass
				texture = true;
				b[15] = 0b0001110000011100;
				b[14] = 0b0011111000111110;
				b[13] = 0b0011011000110110;
				b[12] = 0b0011111000111110;
				b[11] = 0b0001110000011100;
				b[10] = 0b0000000000000000;
				b[9]  = 0b0000000000000000;
				b[8]  = 0b0000000000000000;
				b[7]  = 0b0001110000011100;
				b[6]  = 0b0011111000111110;
				b[5]  = 0b0011011000110110;
				b[4]  = 0b0011111000111110;
				b[3]  = 0b0001110000011100;
				b[2]  = 0b0000000000000000;
				b[1]  = 0b0000000000000000;
				b[0]  = 0b0000000000000000;
				break;
			case "electrode": // aka. poly2
				texture = true;
				b[15] = 0b1010111110101111;
				b[14] = 0b1000100010001000;
				b[13] = 0b1111101011111010;
				b[12] = 0b1000100010001000;
				b[11] = 0b1010111110101111;
				b[10] = 0b1000100010001000;
				b[9]  = 0b1111101011111010;
				b[8]  = 0b1000100010001000;
				b[7]  = 0b1010111110101111;
				b[6]  = 0b1000100010001000;
				b[5]  = 0b1111101011111010;
				b[4]  = 0b1000100010001000;
				b[3]  = 0b1010111110101111;
				b[2]  = 0b1000100010001000;
				b[1]  = 0b1111101011111010;
				b[0]  = 0b1000100010001000;
				break;
			case "p-base":
				texture = true;
				b[15] = 0b0100010001000100;
				b[14] = 0b0010001000100010;
				b[13] = 0b0001000100010001;
				b[12] = 0b1000100010001000;
				b[11] = 0b0100010001000100;
				b[10] = 0b0010001000100010;
				b[9]  = 0b0001000100010001;
				b[8]  = 0b0000100010001000;
				b[7]  = 0b0100010001000100;
				b[6]  = 0b0010001000100010;
				b[5]  = 0b0001000100010001;
				b[4]  = 0b1000100010001000;
				b[3]  = 0b0100010001000100;
				b[2]  = 0b0010001000100010;
				b[1]  = 0b0001000100010001;
				b[0]  = 0b0000100010001000;
				break;
			case "hi-res":
			case "selected":
				texture = true;
				b[15] = 0b0000000000000000;
				b[14] = 0b0000000000000000;
				b[13] = 0b0000000000000000;
				b[12] = 0b0000000000000000;
				b[11] = 0b0000000000000000;
				b[10] = 0b0000000000000000;
				b[9]  = 0b0000000000000000;
				b[8]  = 0b0000000000000000;
				b[7]  = 0b0000000000000000;
				b[6]  = 0b0000000000000000;
				b[5]  = 0b0000000000000000;
				b[4]  = 0b0000000000000000;
				b[3]  = 0b0000000000000000;
				b[2]  = 0b0000000000000000;
				b[1]  = 0b0000000000000000;
				b[0]  = 0b0000000000000000;
				break;
			}
			if (!texture)
				return new SolidBrush(c);
			for (int i = 0, k = 0; i < 16; i++)
			{
				for (int j = 0x8000; j > 0; j >>= 1, k += 4)
				{
					if ((b[i] & j) != 0)
					{
						arr[k]     = c.B;
						arr[k + 1] = c.G;
						arr[k + 2] = c.R;
						arr[k + 3] = c.A;
					}
				}
			}
			unsafe
			{
				fixed (byte* p = arr)
				{
					Bitmap img = new Bitmap(16, 16, 16 * 4, PixelFormat.Format32bppArgb, (IntPtr)p);
					return new TextureBrush(img);
				}
			}
		}
	}
}
