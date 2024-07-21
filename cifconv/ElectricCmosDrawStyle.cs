using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace cifconv
{
	public class ElectricCmosDrawStyle : DrawStyle
	{
		public ElectricCmosDrawStyle()
		{
			TransparentLayers = new string[] {
				"metal1",
				"poly",
				"active,p-active,n-active",
				"select,p-select",
				"well,p-well",
			};

			SolidLayers = new string[] {
				"contact",
				"oversize-contact",
				"active-contact",
				"poly-contact",
				"electrode-contact",
				"passivation",
				"selected",
			};

			GenerateElectricColorMap();
		}

		public override Color GetLayerColor(string layer)
		{
			switch (layer)
			{
				case "well":              return Color.FromArgb(0xff, 0xaa, 0x8c, 0x1e);
				case "p-well":            return Color.FromArgb(0xff, 0xaa, 0x8c, 0x1e);
				case "active":            return Color.FromArgb(0xff, 0x00, 0xff, 0x00);
				case "p-active":          return Color.FromArgb(0xff, 0x00, 0xff, 0x00);
				case "n-active":          return Color.FromArgb(0xff, 0x00, 0xff, 0x00);
				case "select":            return Color.FromArgb(0xff, 0xff, 0xbe, 0x06);
				case "p-select":          return Color.FromArgb(0xff, 0xff, 0xbe, 0x06);
				case "poly":              return Color.FromArgb(0xff, 0xdf, 0x00, 0x00);
				case "contact":           return Color.FromArgb(0xff, 0xb4, 0x82, 0x00);
				case "oversize-contact":  return Color.FromArgb(0xff, 0xb4, 0x82, 0x00);
				case "active-contact":    return Color.FromArgb(0xff, 0xb4, 0x82, 0x00);
				case "poly-contact":      return Color.FromArgb(0xff, 0xb4, 0x82, 0x00);
				case "electrode-contact": return Color.FromArgb(0xff, 0xb4, 0x82, 0x00);
				case "metal1":            return Color.FromArgb(0xff, 0x00, 0x00, 0xff);
				case "passivation":       return Color.FromArgb(0xff, 0x00, 0x00, 0x00); // aka. glass
				case "selected":          return Color.FromArgb(0xaa, 0x00, 0xff, 0xff);
				default:                  return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
			}
		}

		public override Pen GetLayerPen(string layer)
		{
			switch (layer)
			{
			case "contact":
			case "oversize-contact":
			case "active-contact":
			case "poly-contact":
			case "electrode-contact":
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

		public override Brush GetLayerBrush(string layer)
		{
			Color c = GetLayerColor(layer);
			bool texture = false;
			ushort[] b = new ushort[16];
			switch (layer)
			{
			case "contact":
			case "oversize-contact":
			case "active-contact":
			case "poly-contact":
			case "electrode-contact":
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
			if (texture)
				return BrushFromMono16X16(b, c);
			else
				return new SolidBrush(c);
		}
	}
}
