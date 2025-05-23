using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace cifconv
{
	public class ElectricMocmosDrawStyle : DrawStyle
	{
		private bool print;

		public ElectricMocmosDrawStyle(bool print = false)
		{
			this.print = print;

			TransparentLayers = new string[] {
				"metal1",
				"poly,silicide-block",
				"active,p-active,n-active",
				"metal2",
				"metal3",
			};

			SolidLayers = new string[] {
				"contact",
				"oversize-contact",
				"active-contact",
				"poly-contact",
				"electrode-contact",
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

			GenerateElectricColorMap();
		}

		public override Color GetLayerColor(string layer)
		{
			switch (layer)
			{
				case "well":              return Color.FromArgb(0xff, 0x8b, 0x63, 0x2e);
				case "p-well":            return Color.FromArgb(0xff, 0x8b, 0x63, 0x2e);
				case "n-well":            return Color.FromArgb(0xff, 0x8b, 0x63, 0x2e);
				case "active":            return Color.FromArgb(0xff, 0x6b, 0xe2, 0x60);
				case "p-active":          return Color.FromArgb(0xff, 0x6b, 0xe2, 0x60);
				case "n-active":          return Color.FromArgb(0xff, 0x6b, 0xe2, 0x60);
				case "select":            return Color.FromArgb(0xff, 0xff, 0xff, 0x00);
				case "p-select":          return Color.FromArgb(0xff, 0xff, 0xff, 0x00);
				case "n-select":          return Color.FromArgb(0xff, 0xff, 0xff, 0x00);
				case "poly":              return Color.FromArgb(0xff, 0xff, 0x9b, 0xc0);
				case "contact":           return Color.FromArgb(0xff, 0x64, 0x64, 0x64);
				case "oversize-contact":  return Color.FromArgb(0xff, 0x64, 0x64, 0x64);
				case "active-contact":    return Color.FromArgb(0xff, 0x64, 0x64, 0x64);
				case "poly-contact":      return Color.FromArgb(0xff, 0x64, 0x64, 0x64);
				case "electrode-contact": return Color.FromArgb(0xff, 0x64, 0x64, 0x64);
				case "metal1":            return Color.FromArgb(0xff, 0x60, 0xd1, 0xff);
				case "via1":              return Color.FromArgb(0xff, 0xb4, 0xb4, 0xb4);
				case "metal2":            return Color.FromArgb(0xff, 0xe0, 0x5f, 0xff);
				case "via2":              return Color.FromArgb(0xff, 0xb4, 0xb4, 0xb4);
				case "metal3":            return Color.FromArgb(0xff, 0xf7, 0xfb, 0x14);
				case "via3":              return Color.FromArgb(0xff, 0xb4, 0xb4, 0xb4);
				case "metal4":            return Color.FromArgb(0xff, 0x96, 0x96, 0xff);
				case "via4":              return Color.FromArgb(0xff, 0xb4, 0xb4, 0xb4);
				case "metal5":            return Color.FromArgb(0xff, 0xff, 0xbe, 0x06);
				case "via5":              return Color.FromArgb(0xff, 0xb4, 0xb4, 0xb4);
				case "metal6":            return Color.FromArgb(0xff, 0x00, 0xff, 0xff);
				case "p-high-voltage":    return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "n-high-voltage":    return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "thick-active":      return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "pad":               return Color.FromArgb(0xff, 0xff, 0x00, 0x00);
				case "poly-cap":          return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "silicide-block":    return Color.FromArgb(0xff, 0xff, 0x9b, 0xc0);
				case "passivation":       return Color.FromArgb(0xff, 0x64, 0x64, 0x64); // aka. glass
				case "electrode":         return Color.FromArgb(0xff, 0xff, 0xbe, 0x06); // aka. poly2
				case "p-base":            return Color.FromArgb(0xff, 0x6b, 0xe2, 0x60);
				case "hi-res":            return Color.FromArgb(0xff, 0xff, 0x00, 0x00);
				case "selected":          return Color.FromArgb(0xaa, 0x00, 0xff, 0xff);
				default:                  return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
			}
		}

		public override Pen GetLayerPen(string layer)
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

		public override Brush GetLayerBrush(string layer)
		{
			Color c = GetLayerColor(layer);
			bool texture = false;
			ushort[] b = new ushort[16];
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
			case "metal2":
				texture = layer == "p-select" || print;
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
			case "metal1":
			case "metal3":
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
			case "poly":
				texture = print;
				b[15] = 0b0001000100010001;
				b[14] = 0b1111111111111111;
				b[13] = 0b0001000100010001;
				b[12] = 0b0101010101010101;
				b[11] = 0b0001000100010001;
				b[10] = 0b1111111111111111;
				b[9]  = 0b0001000100010001;
				b[8]  = 0b0101010101010101;
				b[7]  = 0b0001000100010001;
				b[6]  = 0b1111111111111111;
				b[5]  = 0b0001000100010001;
				b[4]  = 0b0101010101010101;
				b[3]  = 0b0001000100010001;
				b[2]  = 0b1111111111111111;
				b[1]  = 0b0001000100010001;
				b[0]  = 0b0101010101010101;
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
			if (texture)
				return BrushFromMono16X16(b, c);
			else
				return new SolidBrush(c);
		}
	}
}
