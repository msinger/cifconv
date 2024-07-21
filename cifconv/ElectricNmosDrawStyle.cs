using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace cifconv
{
	public class ElectricNmosDrawStyle : DrawStyle
	{
		public ElectricNmosDrawStyle()
		{
			TransparentLayers = new string[] {
				"metal1",
				"poly",
				"active,p-active,n-active",
				"implant",
				"buried",
			};

			SolidLayers = new string[] {
				"contact",
				"active-contact",
				"poly-contact",
				"electrode-contact",
				"oversize-contact",
				"passivation",
				"hard-enhancement",
				"light-implant",
				"light-enhancement",
				"selected",
			};

			GenerateElectricColorMap();
		}

		public override Color GetLayerColor(string layer)
		{
			switch (layer)
			{
				case "active":            return Color.FromArgb(0xff, 0x46, 0xfa, 0x46);
				case "p-active":          return Color.FromArgb(0xff, 0x46, 0xfa, 0x46);
				case "n-active":          return Color.FromArgb(0xff, 0x46, 0xfa, 0x46);
				case "poly":              return Color.FromArgb(0xff, 0xdc, 0x00, 0x78);
				case "contact":           return Color.FromArgb(0xff, 0xb4, 0x82, 0x00);
				case "oversize-contact":  return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "active-contact":    return Color.FromArgb(0xff, 0xb4, 0x82, 0x00);
				case "poly-contact":      return Color.FromArgb(0xff, 0xb4, 0x82, 0x00);
				case "electrode-contact": return Color.FromArgb(0xff, 0xb4, 0x82, 0x00);
				case "metal1":            return Color.FromArgb(0xff, 0x00, 0x00, 0xc8);
				case "passivation":       return Color.FromArgb(0xff, 0x00, 0x00, 0x00); // aka. glass
				case "buried":            return Color.FromArgb(0xff, 0xb4, 0xb4, 0xb4);
				case "implant":           return Color.FromArgb(0xff, 0xfa, 0xfa, 0x00);
				case "light-implant":     return Color.FromArgb(0xff, 0x96, 0x5a, 0x00);
				case "hard-enhancement":  return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
				case "light-enhancement": return Color.FromArgb(0xff, 0x00, 0x00, 0x00);
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

		protected override void Draw(IDrawable d, Graphics g, string layer, Pen p, Brush b)
		{
			base.Draw(d, g, layer, p, b);
			if ((layer == "contact" ||
			     layer == "active-contact" ||
			     layer == "poly-contact" ||
			     layer == "electrode-contact") &&
			    d is Box)
			{
				Box box = (Box)d;
				g.DrawLine(p, (float)box.P0.X, (float)box.P0.Y, (float)box.P1.X, (float)box.P1.Y);
				g.DrawLine(p, (float)box.P1.X, (float)box.P0.Y, (float)box.P0.X, (float)box.P1.Y);
			}
		}
	}
}
