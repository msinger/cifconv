using System;
using System.Globalization;
using System.Text;

namespace cifconv
{
	public struct Vector
	{
		public double X, Y;

		public Vector(double x, double y)
		{
			X = x;
			Y = y;
		}

		public static Vector operator  +(Vector v)           => v;
		public static Vector operator  -(Vector v)           => new Vector(-v.X, -v.Y);
		public static Vector operator  +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y);
		public static Vector operator  -(Vector a, Vector b) => new Vector(a.X - b.X, a.Y - b.Y);
		public static Vector operator  *(Vector v, double d) => new Vector(v.X * d, v.Y * d);
		public static Vector operator  *(double d, Vector v) => new Vector(v.X * d, v.Y * d);
		public static double operator  *(Vector a, Vector b) => a.X * b.X + a.Y * b.Y;
		public static Vector operator  /(Vector v, double d) => new Vector(v.X / d, v.Y / d);
		public static bool   operator ==(Vector a, Vector b) => a.X == b.X && a.Y == b.Y;
		public static bool   operator !=(Vector a, Vector b) => !(a == b);

		public bool Equals(Vector o)
		{
			return this == o;
		}

		public override bool Equals(object o)
		{
			if (!(o is Vector))
				return false;
			return Equals((Vector)o);
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		public static int Order(Vector a, Vector b, Vector c)
		{
			// ABC is clockwise:         returns 1
			// ABC is counter-clockwise: returns -1
			// ABC is co-linear:         returns 0
			return Math.Sign((c.Y - a.Y) * (b.X - a.X) - (b.Y - a.Y) * (c.X - a.X));
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("{ X: ");
			sb.Append(X.ToString(CultureInfo.InvariantCulture));
			sb.Append("; Y: ");
			sb.Append(Y.ToString(CultureInfo.InvariantCulture));
			sb.Append(" }");
			return sb.ToString();
		}
	}
}
