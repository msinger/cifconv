using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace cifconv
{
	public abstract class DrawStyle
	{
		protected string[] TransparentLayers;
		protected uint[]   TransparentColors;

		protected string[] SolidLayers;

		protected DrawStyle()
		{
			TransparentLayers = new string[0] { };
			TransparentColors = new uint[0]   { };
			SolidLayers       = new string[0] { };
		}

		public abstract Color GetLayerColor(string layer);
		public abstract Brush GetLayerBrush(string layer);
		public abstract Pen GetLayerPen(string layer);

		protected virtual Bitmap NewBitmap(int width, int height, uint bgcolor = 0)
		{
			Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			if (bgcolor != 0)
			{
				using (Graphics g = Graphics.FromImage(bmp))
				{
					Brush b = new SolidBrush(Color.FromArgb(unchecked((int)bgcolor)));
					g.FillRectangle(b, new Rectangle(0, 0, width, height));
				}
			}
			return bmp;
		}

		protected virtual Graphics NewGraphics(Bitmap bmp)
		{
			Graphics g = Graphics.FromImage(bmp);
			g.ScaleTransform(1.0f, -1.0f);
			return g;
		}

		protected virtual Bitmap BitmapFromTransLayer(Layout layout, string layer, int width, int height)
		{
			Bitmap bmp = NewBitmap(width, height);
			using (Graphics g = NewGraphics(bmp))
			{
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

		protected virtual Bitmap DrawTransparentLayers(Layout layout, int width, int height, uint bgcolor)
		{
			Bitmap bmp;
			if (TransparentLayers.Length >= 1)
				bmp = BitmapFromTransLayer(layout, TransparentLayers[0], width, height);
			else
				bmp = NewBitmap(width, height);
			var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
			                        ImageLockMode.ReadWrite, bmp.PixelFormat);
			for (int i = 1, j = 2; i < TransparentLayers.Length; i++, j <<= 1)
			{
				using (Bitmap bmp2 = BitmapFromTransLayer(layout, TransparentLayers[i], width, height))
				{
					var data2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height),
					                          ImageLockMode.ReadOnly, bmp2.PixelFormat);
					unsafe
					{
						uint* p = (uint*)data.Scan0;
						uint* q = (uint*)data2.Scan0;
						for (int k = 0; k < data.Stride * data.Height / sizeof(uint); k++)
						{
							if (q[k] != 0)
							{
								for (int l = j, m = 0; m < j; l++, m++)
								{
									if (p[k] == TransparentColors[m])
									{
										p[k] = TransparentColors[l];
										break;
									}
								}
							}
						}
					}
					bmp2.UnlockBits(data2);
				}
			}
			unsafe
			{
				uint* p = (uint*)data.Scan0;
				for (int i = 0; i < data.Stride * data.Height / sizeof(uint); i++)
					if (p[i] == 0)
						p[i] = bgcolor;
			}
			bmp.UnlockBits(data);
			return bmp;
		}

		protected virtual void DrawSolidLayers(Layout layout, Bitmap bmp)
		{
			using (Graphics g = NewGraphics(bmp))
			{
				foreach (var l in SolidLayers)
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
		}

		public virtual Bitmap DrawLayout(Layout layout, int width, int height, uint bgcolor)
		{
			Bitmap bmp = DrawTransparentLayers(layout, width, height, bgcolor);
			DrawSolidLayers(layout, bmp);
			return bmp;
		}

		public virtual Bitmap DrawLayer(Layout layout, string layer, int width, int height, uint bgcolor)
		{
			Pen   p = GetLayerPen(layer);
			Brush b = GetLayerBrush(layer);
			List<IDrawable> l = new List<IDrawable>();
			if (layout.Layers.ContainsKey(layer))
				l = layout.Layers[layer];
			else
				Console.Error.WriteLine("Layer " + layer + " is empty.");
			Bitmap bmp = NewBitmap(width, height, bgcolor);
			using (Graphics g = NewGraphics(bmp))
			{
				foreach (var d in l)
					d.Draw(g, p, b);
			}
			return bmp;
		}

		protected virtual TextureBrush BrushFromMono16X16(ushort[] buf, Color color)
		{
			int[] arr = new int[16 * 16];
			int c = color.ToArgb();
			for (int i = 0, k = 0; i < 16; i++)
				for (int j = 0x8000; j > 0; j >>= 1, k++)
					if ((buf[i] & j) != 0)
						arr[k] = c;
			unsafe
			{
				fixed (int* p = arr)
				{
					Bitmap img = new Bitmap(16, 16, 16 * 4, PixelFormat.Format32bppArgb, (IntPtr)p);
					return new TextureBrush(img);
				}
			}
		}
	}
}
