using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace cifconv
{
	public class ElectricRcmosDrawStyle : DrawStyle
	{
		private bool print;

		public ElectricRcmosDrawStyle(bool print = false)
		{
			this.print = print;

			TransparentLayers = new string[] {
				"metal1",
				"poly",
				"active,p-active,n-active",
				"well",
				"metal2",
			};

			SolidLayers = new string[] {
				"contact",
				"oversize-contact",
				"active-contact",
				"poly-contact",
				"electrode-contact",
				"via1",
				"passivation",
				"select",
				"selected",
			};

			GenerateElectricColorMap();
		}

		public override Color GetLayerColor(string layer)
		{
			switch (layer)
			{
				case "well":              return Color.FromArgb(0xff, 0xf0, 0xdd, 0xb5);
				case "active":            return Color.FromArgb(0xff, 0x6b, 0xe2, 0x60);
				case "p-active":          return Color.FromArgb(0xff, 0x6b, 0xe2, 0x60);
				case "n-active":          return Color.FromArgb(0xff, 0x6b, 0xe2, 0x60);
				case "select":            return Color.FromArgb(0xff, 0xff, 0xff, 0x00);
				case "poly":              return Color.FromArgb(0xff, 0xff, 0x9b, 0xc0);
				case "contact":           return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "oversize-contact":  return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "active-contact":    return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "poly-contact":      return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "electrode-contact": return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "metal1":            return Color.FromArgb(0xff, 0x60, 0xd1, 0xff);
				case "via1":              return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "metal2":            return Color.FromArgb(0xff, 0xe0, 0x5f, 0xff);
				case "passivation":       return Color.FromArgb(0xff, 0x64, 0x64, 0x64); // aka. glass
				case "selected":          return Color.FromArgb(0xaa, 0x00, 0xff, 0xff);
				default:                  return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
			}
		}

		public override Pen GetLayerPen(string layer)
		{
			switch (layer)
			{
			case "selected":
				Color c = GetLayerColor(layer);
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
			case "well":
				texture = print;
				b[15] = 0b0000000000000000;
				b[14] = 0b0000000011000000;
				b[13] = 0b0000000000000000;
				b[12] = 0b0000000000000000;
				b[11] = 0b0000000000000000;
				b[10] = 0b0000000011000000;
				b[9]  = 0b0000000000000000;
				b[8]  = 0b0000000000000000;
				b[7]  = 0b0000000000000000;
				b[6]  = 0b0000000011000000;
				b[5]  = 0b0000000000000000;
				b[4]  = 0b0000000000000000;
				b[3]  = 0b0000000000000000;
				b[2]  = 0b0000000011000000;
				b[1]  = 0b0000000000000000;
				b[0]  = 0b0000000000000000;
				break;
			case "metal1":
				texture = print;
				b[15] = 0b0010001000100010;
				b[14] = 0b0000000000000000;
				b[13] = 0b1000100010001000;
				b[12] = 0b0000000000000000;
				b[11] = 0b0010001000100010;
				b[10] = 0b0000000000000000;
				b[9]  = 0b1000100010001000;
				b[8]  = 0b0000000000000000;
				b[7]  = 0b0010001000100010;
				b[6]  = 0b0000000000000000;
				b[5]  = 0b1000100010001000;
				b[4]  = 0b0000000000000000;
				b[3]  = 0b0010001000100010;
				b[2]  = 0b0000000000000000;
				b[1]  = 0b1000100010001000;
				b[0]  = 0b0000000000000000;
				break;
			case "select":
			case "metal2":
				texture = layer == "select" || print;
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
			case "poly":
				texture = print;
				b[15] = 0b0000100000001000;
				b[14] = 0b0000010000000100;
				b[13] = 0b0000001000000010;
				b[12] = 0b0000000100000001;
				b[11] = 0b1000000010000000;
				b[10] = 0b0100000001000000;
				b[9]  = 0b0010000000100000;
				b[8]  = 0b0001000000010000;
				b[7]  = 0b0000100000001000;
				b[6]  = 0b0000010000000100;
				b[5]  = 0b0000001000000010;
				b[4]  = 0b0000000100000001;
				b[3]  = 0b1000000010000000;
				b[2]  = 0b0100000001000000;
				b[1]  = 0b0010000000100000;
				b[0]  = 0b0001000000010000;
				break;
			case "active":
			case "p-active":
			case "n-active":
				texture = print;
				b[15] = 0b0000000000000000;
				b[14] = 0b0000001100000011;
				b[13] = 0b0100100001001000;
				b[12] = 0b0000001100000011;
				b[11] = 0b0000000000000000;
				b[10] = 0b0011000000110000;
				b[9]  = 0b1000010010000100;
				b[8]  = 0b0011000000110000;
				b[7]  = 0b0000000000000000;
				b[6]  = 0b0000001100000011;
				b[5]  = 0b0100100001001000;
				b[4]  = 0b0000001100000011;
				b[3]  = 0b0000000000000000;
				b[2]  = 0b0011000000110000;
				b[1]  = 0b1000010010000100;
				b[0]  = 0b0011000000110000;
				break;
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
