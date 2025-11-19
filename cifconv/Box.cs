using System;
using System.Text;
using System.Drawing;

namespace cifconv
{
	public struct Box : IDrawable
	{
		public Vector P0, P1;

		public static bool RoundGrowing = false;

		public Box(Vector p0, Vector p1)
		{
			P0 = p0;
			P1 = p1;
		}

		public object Clone()
		{
			return new Box(P0, P1);
		}

		public Box Normalize()
		{
			double x0 = Math.Min(P0.X, P1.X);
			double x1 = Math.Max(P0.X, P1.X);
			double y0 = Math.Min(P0.Y, P1.Y);
			double y1 = Math.Max(P0.Y, P1.Y);
			return new Box(new Vector(x0, y0), new Vector(x1, y1));
		}

		public bool Contains(Vector v)
		{
			Box n = Normalize();
			return (v.X >= n.P0.X && v.X <= n.P1.X && v.Y >= n.P0.Y && v.Y <= n.P1.Y);
		}

		public bool Intersects(Box o)
		{
			if (Contains(o.P0) || Contains(o.P1) || o.Contains(P0) || o.Contains(P1))
				return true;
			if (Contains(new Vector(o.P0.X, o.P1.Y)) || Contains(new Vector(o.P1.X, o.P0.Y)))
				return true;
			if (o.Contains(new Vector(P0.X, P1.Y)) || o.Contains(new Vector(P1.X, P0.Y)))
				return true;
			if ((new Line(P0, new Vector(P0.X, P1.Y))).SegmentsIntersect(new Line(o.P0, new Vector(o.P1.X, o.P0.Y))))
				return true;
			if ((new Line(o.P0, new Vector(o.P0.X, o.P1.Y))).SegmentsIntersect(new Line(P0, new Vector(P1.X, P0.Y))))
				return true;
			return false;
		}

		public bool Intersects(Round r)
		{
			return r.Intersects(this);
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
			if (Contains(l.P0))
				return true;
			if (l.SegmentsIntersect(new Line(P0, new Vector(P0.X, P1.Y))))
				return true;
			if (l.SegmentsIntersect(new Line(P1, new Vector(P1.X, P0.Y))))
				return true;
			if (l.SegmentsIntersect(new Line(P0, new Vector(P1.X, P0.Y))))
				return true;
			if (l.SegmentsIntersect(new Line(P1, new Vector(P0.X, P1.Y))))
				return true;
			return false;
		}

		public bool SegmentIntersects(Line l, double maxDist)
		{
			// maxDist * 2 == line width
			Vector p0 = new Vector(Math.Min(P0.X, P1.X) - maxDist, Math.Min(P0.Y, P1.Y) - maxDist);
			Vector p1 = new Vector(Math.Max(P0.X, P1.X) + maxDist, Math.Max(P0.Y, P1.Y) + maxDist);
			Box b = new Box(p0, p1);
			return b.SegmentIntersects(l);
		}

		public void Draw(Graphics g, Pen p, Brush b)
		{
			Box n = Normalize();
			RectangleF r = new RectangleF((float)n.P0.X, (float)n.P0.Y, (float)(n.P1.X - n.P0.X), (float)(n.P1.Y - n.P0.Y));
			g.DrawRectangle(p, Rectangle.Round(r));
			g.FillRectangle(b, r);
		}

		public Box GetAABB()
		{
			return Normalize();
		}

		public Polygon ToPolygon()
		{
			Box b = Normalize();
			Polygon p = new Polygon();
			p.P.Add(b.P0);
			p.P.Add(new Vector(b.P1.X, b.P0.Y));
			p.P.Add(b.P1);
			p.P.Add(new Vector(b.P0.X, b.P1.Y));
			return p;
		}

		public void Scale(double scale)
		{
			P0 *= scale;
			P1 *= scale;
		}

		public void FlipY()
		{
			P0.Y *= -1.0;
			P1.Y *= -1.0;
		}

		public void Translate(Vector v)
		{
			P0 += v;
			P1 += v;
		}

		public void MakeIntegerCoords()
		{
			if (RoundGrowing)
			{
				// Round smaller coords down and larger coords up, so we don't get gaps between two adjacent boxes.
				if (P0.X < P1.X)
				{
					P0.X = Math.Floor(P0.X);
					P1.X = Math.Ceiling(P1.X);
				}
				else
				{
					P0.X = Math.Ceiling(P0.X);
					P1.X = Math.Floor(P1.X);
				}
				if (P0.Y < P1.Y)
				{
					P0.Y = Math.Floor(P0.Y);
					P1.Y = Math.Ceiling(P1.Y);
				}
				else
				{
					P0.Y = Math.Ceiling(P0.Y);
					P1.Y = Math.Floor(P1.Y);
				}
			}
			else
			{
				P0.X = Math.Round(P0.X);
				P0.Y = Math.Round(P0.Y);
				P1.X = Math.Round(P1.X);
				P1.Y = Math.Round(P1.Y);
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("{ P0: ");
			sb.Append(P0.ToString());
			sb.Append("; P1: ");
			sb.Append(P1.ToString());
			sb.Append(" }");
			return sb.ToString();
		}
	}
}
