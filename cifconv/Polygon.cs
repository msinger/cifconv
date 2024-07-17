using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace cifconv
{
	public class Polygon : IDrawable
	{
		public List<Vector> P;

		public Polygon()
		{
			P = new List<Vector>();
		}

		public object Clone()
		{
			Polygon c = new Polygon();
			c.P.AddRange(P);
			return c;
		}

		public bool Contains(Vector v)
		{
			if (P.Count == 0)
				return false;
			if (P.Count == 1)
				return P[0] == v;
			int winding = 0;
			for (int i = 0; i < P.Count; i++)
			{
				Vector a = P[i];
				Vector b = P[(i + 1) % P.Count];
				Line e = new Line(a, b);
				if (e.SegmentIntersects(v))
					return true;
				if (a.Y == b.Y)
					continue;
				bool up   = v.Y >= a.Y && v.Y < b.Y;
				bool down = v.Y >= b.Y && v.Y < a.Y;
				double x = a.X + (v.Y - a.Y) * (b.X - a.X) / (b.Y - a.Y);
				if (x < v.X)
					continue;
				if (up)
					winding++;
				if (down)
					winding--;
			}
			return winding != 0;
		}

		public bool Intersects(Polygon o)
		{
			if (P.Count == 0)
				return false;
			if (o.P.Count == 0)
				return false;
			if (Contains(o.P[0]))
				return true;
			if (o.Contains(P[0]))
				return true;
			if (P.Count == 1)
				return false;
			if (o.P.Count == 1)
				return false;
			if (!GetAABB().Intersects(o.GetAABB()))
				return false;
			for (int i = 0; i < P.Count; i++)
			{
				Vector a = P[i];
				Vector b = P[(i + 1) % P.Count];
				Line e = new Line(a, b);
				for (int j = 0; j < o.P.Count; j++)
				{
					Vector c = o.P[j];
					Vector d = o.P[(j + 1) % o.P.Count];
					Line f = new Line(c, d);
					if (e.SegmentsIntersect(f))
						return true;
				}
			}
			return false;
		}

		public bool Intersects(Box r)
		{
			if (P.Count == 0)
				return false;
			if (r.Contains(P[0]))
				return true;
			if (P.Count == 1)
				return false;
			if (Contains(r.P0))
				return true;
			for (int i = 0; i < P.Count; i++)
			{
				Vector a = P[i];
				Vector b = P[(i + 1) % P.Count];
				Line e = new Line(a, b);
				if (e.SegmentsIntersect(new Line(r.P0, new Vector(r.P0.X, r.P1.Y))))
					return true;
				if (e.SegmentsIntersect(new Line(r.P0, new Vector(r.P1.X, r.P0.Y))))
					return true;
				if (e.SegmentsIntersect(new Line(r.P1, new Vector(r.P0.X, r.P1.Y))))
					return true;
				if (e.SegmentsIntersect(new Line(r.P1, new Vector(r.P1.X, r.P0.Y))))
					return true;
			}
			return false;
		}

		public bool Intersects(Round r)
		{
			if (P.Count == 0)
				return false;
			if (P.Count == 1)
				return r.Contains(P[0]);
			if (Contains(r.Center))
				return true;
			for (int i = 0; i < P.Count; i++)
			{
				Vector a = P[i];
				Vector b = P[(i + 1) % P.Count];
				Line e = new Line(a, b);
				if (r.SegmentIntersects(e))
					return true;
			}
			return false;
		}

		public bool Intersects(IDrawable d)
		{
			return d.Intersects(this);
		}

		public bool SegmentIntersects(Line l)
		{
			if (P.Count == 0)
				return false;
			if (P.Count == 1)
				return l.SegmentIntersects(P[0]);
			if (Contains(l.P0))
				return true;
			for (int i = 0; i < P.Count; i++)
			{
				Vector a = P[i];
				Vector b = P[(i + 1) % P.Count];
				Line e = new Line(a, b);
				if (l.SegmentsIntersect(e))
					return true;
			}
			return false;
		}

		public void Draw(Graphics g, Pen p, Brush b)
		{
			System.Drawing.Point[] a = new System.Drawing.Point[P.Count];
			for (int i = 0; i < P.Count; i++)
				a[i] = new System.Drawing.Point((int)Math.Round(P[i].X), (int)Math.Round(P[i].Y));
			g.DrawPolygon(p, a);
			g.FillPolygon(b, a, FillMode.Winding);
		}

		public Box GetAABB()
		{
			double minX = 0.0;
			double minY = 0.0;
			double maxX = 0.0;
			double maxY = 0.0;

			bool hasCoords = false;

			foreach (Vector v in P)
			{
				if (!hasCoords)
				{
					minX = v.X;
					maxX = v.X;
					minY = v.Y;
					maxY = v.Y;
					hasCoords = true;
					continue;
				}
				minX = (minX < v.X) ? minX : v.X;
				minY = (minY < v.Y) ? minY : v.Y;
				maxX = (maxX > v.X) ? maxX : v.X;
				maxY = (maxY > v.Y) ? maxY : v.Y;
			}

			return new Box(new Vector(minX, minY), new Vector(maxX, maxY));
		}

		public void Scale(double scale)
		{
			for (int i = 0; i < P.Count; i++)
				P[i] *= scale;
		}

		public void Translate(Vector v)
		{
			for (int i = 0; i < P.Count; i++)
				P[i] += v;
		}

		public void MakeIntegerCoords()
		{
			for (int i = 0; i < P.Count; i++)
				P[i] = new Vector(Math.Round(P[i].X), Math.Round(P[i].Y));
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("{");
			for (int i = 0; i < P.Count; i++)
			{
				if (i != 0)
					sb.Append(";");
				sb.Append(" P");
				sb.Append(i.ToString(CultureInfo.InvariantCulture));
				sb.Append(": ");
				sb.Append(P[i].ToString());
			}
			sb.Append(" }");
			return sb.ToString();
		}
	}
}
