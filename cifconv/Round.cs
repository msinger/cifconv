using System;
using System.Text;
using System.Drawing;

namespace cifconv
{
	public struct Round : IDrawable
	{
		public Vector Center;
		public double Radius;

		public static bool RoundGrowing = false;

		public Round(Vector center, double radius)
		{
			Center = center;
			Radius = radius;
		}

		public object Clone()
		{
			return new Round(Center, Radius);
		}

		public bool Contains(Vector v)
		{
			return Math.Sqrt(Math.Pow(Center.X - v.X, 2.0) + Math.Pow(Center.Y - v.Y, 2.0)) <= Radius;
		}

		public bool Intersects(Round o)
		{
			return (new Round(Center, Radius + o.Radius)).Contains(o.Center);
		}

		public bool Intersects(Box b)
		{
			if (b.Contains(Center))
				return true;
			if (SegmentIntersects(new Line(b.P0, new Vector(b.P0.X, b.P1.Y))))
				return true;
			if (SegmentIntersects(new Line(b.P0, new Vector(b.P1.X, b.P0.Y))))
				return true;
			if (SegmentIntersects(new Line(new Vector(b.P0.X, b.P1.Y), b.P1)))
				return true;
			if (SegmentIntersects(new Line(new Vector(b.P1.X, b.P0.Y), b.P1)))
				return true;
			return false;
		}

		public bool Intersects(Polygon p)
		{
			return p.Intersects(this);
		}

		public bool Intersects(IDrawable d)
		{
			return d.Intersects(this);
		}

		public bool SegmentIntersects(Line l)
		{
			if (Contains(l.P0) || Contains(l.P1))
				return true;
			Vector a = l.P0;
			Vector b = l.P1;
			Vector c = Center;
			Vector ab = b - a;
			Vector ac = c - a;
			double t = ((ab * ac) / (ab * ab));
			if (t < 0.0 || t > 1.0)
				return false;
			return Contains(a + ab * t);
		}

		public bool SegmentIntersects(Line l, double maxDist)
		{
			// maxDist * 2 == line width
			Round r = new Round(Center, Radius + maxDist);
			return r.SegmentIntersects(l);
		}

		public void Draw(Graphics g, Pen p, Brush b)
		{
			float x = (float)Center.X;
			float y = (float)Center.Y;
			float r = (float)Radius;
			float d = r * 2.0f;
			g.DrawEllipse(p, x - r, y - r, d, d);
			g.FillEllipse(b, x - r, y - r, d, d);
		}

		public Box GetAABB()
		{
			double x = Center.X;
			double y = Center.Y;
			double r = Radius;
			return new Box(new Vector(x - r, y - r), new Vector(x + r, y + r));
		}

		public void Scale(double scale)
		{
			Center *= scale;
			Radius *= scale;
		}

		public void FlipY()
		{
			Center *= -1.0;
		}

		public void Translate(Vector v)
		{
			Center += v;
		}

		public void MakeIntegerCoords()
		{
			if (RoundGrowing)
				Radius = Math.Ceiling(Radius * 2.0) / 2.0;
			else
				Radius = Math.Round(Radius * 2.0) / 2.0;
			Center.X = Math.Round(Center.X * 2.0) / 2.0;
			Center.Y = Math.Round(Center.Y * 2.0) / 2.0;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("{ C: ");
			sb.Append(Center.ToString());
			sb.Append("; R: ");
			sb.Append(Radius.ToString());
			sb.Append(" }");
			return sb.ToString();
		}
	}
}
