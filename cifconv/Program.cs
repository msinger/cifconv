using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
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
			string styleStr     = null;
			string layer        = null;
			string whitelistStr = null;
			string blacklistStr = null;
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
						case "--style":                           styleStr     = nextArg; i++; break;
						case "--layer":                           layer        = nextArg; i++; break;
						case "--whitelist":                       whitelistStr = nextArg; i++; break;
						case "--blacklist":                       blacklistStr = nextArg; i++; break;
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

			DrawStyle style = GetDrawStyle(styleStr);
			if (style == null)
			{
				Console.Error.WriteLine("Invalid style: " + styleStr);
				return 1;
			}

			if (!string.IsNullOrEmpty(layer) && !IsLayerValid(layer))
			{
				Console.Error.WriteLine("Invalid layer: " + layer);
				return 1;
			}

			string[] whitelist = null;
			string[] blacklist = null;
			if (!string.IsNullOrEmpty(whitelistStr))
			{
				whitelist = whitelistStr.Split(new char[] { ';', ',', ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string s in whitelist)
				{
					if (!IsLayerValid(s))
					{
						Console.Error.WriteLine("Invalid layer in whitelist: " + s);
						return 1;
					}
				}
			}
			if (!string.IsNullOrEmpty(blacklistStr))
			{
				blacklist = blacklistStr.Split(new char[] { ';', ',', ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string s in blacklist)
				{
					if (!IsLayerValid(s))
					{
						Console.Error.WriteLine("Invalid layer in blacklist: " + s);
						return 1;
					}
				}
			}
			List<string> drawnLayers = new List<string>();
			if (whitelist != null)
				drawnLayers.AddRange(whitelist);
			else
				drawnLayers.AddRange(validLayers);
			drawnLayers.Add("selected");
			if (blacklist != null)
				drawnLayers.RemoveAll(s => Array.IndexOf(blacklist, s) != -1);

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
					if (!IsLayerValid(atLayer))
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
				Bitmap bmp;
				Stream s = Console.OpenStandardOutput();
				if (!string.IsNullOrEmpty(outPng) && outPng != "-")
					s = File.Create(outPng);
				if (string.IsNullOrEmpty(layer))
					bmp = style.DrawLayout(layout, width, height, bg, drawnLayers);
				else
					bmp = style.DrawLayer(layout, layer, width, height, bg);
				bmp.Save(s, ImageFormat.Png);
				s.Flush();
			}

			return 0;
		}

		private static DrawStyle GetDrawStyle(string style)
		{
			switch (style)
			{
				case "electric-cmos":         return new ElectricCmosDrawStyle();
				case null:
				case "":
				case "electric-mocmos":       return new ElectricMocmosDrawStyle();
				case "electric-mocmos-print": return new ElectricMocmosDrawStyle(true);
				case "electric-nmos":         return new ElectricNmosDrawStyle();
				case "electric-rcmos":        return new ElectricRcmosDrawStyle();
				case "electric-rcmos-print":  return new ElectricRcmosDrawStyle(true);
				case "mask":                  return new MaskDrawStyle();
				default:                      return null;
			}
		}

		private static void PrintHelp()
		{
			Console.Error.WriteLine("Usage: cifconv.exe [<OPTIONS>] [<FILES>]");
			Console.Error.WriteLine();
			Console.Error.WriteLine("OPTIONS:");
			Console.Error.WriteLine("  --png <FILE>                 Convert CIF to PNG containing everything.");
			Console.Error.WriteLine("  --style <STYLE>              Choose drawing style of PNG.");
			Console.Error.WriteLine("  --layer <LAYER>              Select one layer to operate on. If not given,");
			Console.Error.WriteLine("                               all layers are selected.");
			Console.Error.WriteLine("  --whitelist <LAYERS>         List of layers to draw.");
			Console.Error.WriteLine("  --blacklist <LAYERS>         List of layers not to draw.");
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

		private static string[] validLayers = new string[] {
			"well",
			"p-well",
			"n-well",
			"active",
			"p-active",
			"n-active",
			"select",
			"p-select",
			"n-select",
			"poly",
			"contact",
			"oversize-contact",
			"active-contact",
			"poly-contact",
			"electrode-contact",
			"metal1",
			"via1",
			"metal2",
			"via2",
			"metal3",
			"via3",
			"metal4",
			"via4",
			"metal5",
			"via5",
			"metal6",
			"p-high-voltage",
			"n-high-voltage",
			"thick-active",
			"mems-open",
			"mems-etch-stop",
			"pad",
			"exp-field-impl",
			"poly-cap",
			"silicide-block",
			"passivation",
			"electrode",
			"buried",
			"p-base",
			"cap-well",
			"implant",
			"light-implant",
			"hard-enhancement",
			"light-enhancement",
			"hi-res",
		};

		private static bool IsLayerValid(string layer)
		{
			return Array.IndexOf(validLayers, layer) != -1;
		}
	}
}
