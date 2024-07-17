using System.Collections.Generic;

namespace cifconv
{
	public class Layout
	{
		public readonly Dictionary<string, List<IDrawable>> Layers;

		public Layout(Dictionary<string, List<IDrawable>> layers)
		{
			Layers = layers;
		}

		public Box GetAABB()
		{
			double minX = 0.0;
			double minY = 0.0;
			double maxX = 0.0;
			double maxY = 0.0;

			bool hasCoords = false;

			foreach (List<IDrawable> l in Layers.Values)
			{
				foreach (IDrawable d in l)
				{
					Box aabb = d.GetAABB();
					if (!hasCoords)
					{
						minX = aabb.P0.X;
						maxX = aabb.P1.X;
						minY = aabb.P0.Y;
						maxY = aabb.P1.Y;
						hasCoords = true;
						continue;
					}
					minX = (minX < aabb.P0.X) ? minX : aabb.P0.X;
					minY = (minY < aabb.P0.Y) ? minY : aabb.P0.Y;
					maxX = (maxX > aabb.P1.X) ? maxX : aabb.P1.X;
					maxY = (maxY > aabb.P1.Y) ? maxY : aabb.P1.Y;
				}
			}

			return new Box(new Vector(minX, minY), new Vector(maxX, maxY));
		}

		public void Scale(double scale)
		{
			foreach (List<IDrawable> l in Layers.Values)
				foreach (IDrawable d in l)
					d.Scale(scale);
		}

		public void Translate(Vector v)
		{
			foreach (List<IDrawable> l in Layers.Values)
				foreach (IDrawable d in l)
					d.Translate(v);
		}

		public void MakeIntegerCoords()
		{
			foreach (List<IDrawable> l in Layers.Values)
				foreach (IDrawable d in l)
					d.MakeIntegerCoords();
		}

		public Dictionary<string, List<int>> GetShapesAtCoord(Vector v, string layer)
		{
			Dictionary<string, List<int>> r = new Dictionary<string, List<int>>();
			foreach (var kvp in Layers)
			{
				var l = kvp.Value;
				var k = kvp.Key;
				if (layer != null && k != layer)
					continue;
				for (int i = 0; i < l.Count; i++)
				{
					var d = l[i];
					if (d.Contains(v))
					{
						if (!r.ContainsKey(k))
							r.Add(k, new List<int>());
						r[k].Add(i);
					}
				}
			}
			return r;
		}

		public bool HasNonIntegerCoords()
		{
			foreach (List<IDrawable> l in Layers.Values)
			{
				foreach (IDrawable d in l)
				{
					if (d is Box)
					{
						Box b = (Box)d;
						if ((b.P0.X % 1.0 != 0.0) || (b.P0.Y % 1.0 != 0.0) ||
						    (b.P1.X % 1.0 != 0.0) || (b.P1.Y % 1.0 != 0.0))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
