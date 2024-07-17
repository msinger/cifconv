using System;

namespace cifconv
{
	public struct Line
	{
		public Vector P0, P1;

		public Line(Vector p0, Vector p1)
		{
			P0 = p0;
			P1 = p1;
		}

		public int GetSide(Vector p)
		{
			// p is on right side: 1
			// p is on left side: -1
			return Vector.Order(P0, P1, p);
		}

		public static bool SegmentsIntersect(Line a, Line b)
		{
			bool ccw_acd = Vector.Order(a.P0, b.P0, b.P1) < 0;
			bool ccw_bcd = Vector.Order(a.P1, b.P0, b.P1) < 0;
			bool ccw_abc = Vector.Order(a.P0, a.P1, b.P0) < 0;
			bool ccw_abd = Vector.Order(a.P0, a.P1, b.P1) < 0;
			return ccw_acd != ccw_bcd && ccw_abc != ccw_abd;
		}

		public bool SegmentsIntersect(Line o)
		{
			return Line.SegmentsIntersect(this, o);
		}

		public bool SegmentIntersects(Vector v)
		{
			Box b = new Box(P0, P1);
			if (!b.Contains(v))
				return false;
			if (Vector.Order(P0, P1, v) == 0)
				return true;
			return false;
		}

		public bool IsClose(Vector v, double maxDist)
		{
			// maxDist * 2 == line width
			Vector p0 = new Vector(Math.Min(P0.X, P1.X) - maxDist, Math.Min(P0.Y, P1.Y) - maxDist);
			Vector p1 = new Vector(Math.Max(P0.X, P1.X) + maxDist, Math.Max(P0.Y, P1.Y) + maxDist);
			Box b = new Box(p0, p1);
			if (!b.Contains(v))
				return false;
			double dist = Math.Abs((P1.X - P0.X) * (P0.Y - v.Y) - (P0.X - v.X) * (P1.Y - P0.Y)) /
				Math.Sqrt(Math.Pow(P1.X - P0.X, 2.0) + Math.Pow(P1.Y - P0.Y, 2.0));
			return dist <= maxDist;
		}
	}
}
