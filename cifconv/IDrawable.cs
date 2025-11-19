using System;
using System.Drawing;

namespace cifconv
{
	public interface IDrawable : IIntersectable, ICloneable
	{
		void Draw(Graphics g, Pen p, Brush b);
		void Scale(double scale);
		void FlipY();
		void Translate(Vector v);
		void MakeIntegerCoords();
	}
}
