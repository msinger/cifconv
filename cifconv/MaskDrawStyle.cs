using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace cifconv
{
	public class MaskDrawStyle : DrawStyle
	{
		public MaskDrawStyle()
		{
			SolidLayers = new string[] {
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
				"selected",
			};
		}

		public override Color GetLayerColor(string layer)
		{
			switch (layer)
			{
				case "selected":       return Color.FromArgb(0xff, 0xff, 0xff, 0xff);
				default:               return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
			}
		}

		public override Pen GetLayerPen(string layer)
		{
			return Pens.Transparent;
		}

		public override Brush GetLayerBrush(string layer)
		{
			Color c = GetLayerColor(layer);
			return new SolidBrush(c);
		}
	}
}
